using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ThreeL.ContextAPI.Application.Contract.Configurations;
using ThreeL.ContextAPI.Application.Contract.Dtos.File;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Domain.Aggregates.File;
using ThreeL.Infra.Core.Cryptography;
using ThreeL.Infra.Repository.IRepositories;

namespace ThreeL.ContextAPI.Application.Impl.Services
{
    public class FileService : IFileService, IAppService
    {
        private readonly FileStorageOptions _storageOptions;
        private readonly IAdoQuerierRepository<ContextAPIDbContext> _adoQuerierRepository;
        private readonly IAdoExecuterRepository<ContextAPIDbContext> _adoExecuterRepository;
        public FileService(IOptions<FileStorageOptions> storageOptions,
                           IAdoQuerierRepository<ContextAPIDbContext> adoQuerierRepository,
                           IAdoExecuterRepository<ContextAPIDbContext> adoExecuterRepository)
        {
            _storageOptions = storageOptions.Value;
            _adoQuerierRepository = adoQuerierRepository;
            _adoExecuterRepository = adoExecuterRepository;
        }

        public async Task<CheckFileExistResponseDto> CheckFileExistInServerAsync(string code, long userId)
        {
            var resp = new CheckFileExistResponseDto();
            var record =
                await _adoQuerierRepository.QueryFirstOrDefaultAsync<FileRecord>("SELECT TOP 1 * FROM [FILE] WHERE CODE = @Code AND CREATEBY = @UserId",
                new { Code = code, UserId = userId });

            if (record != null && File.Exists(record.Location)) //云存储则需要用其他判断文件存在的方式,一般是接口调用
            {
                resp.Exist = true;
                resp.FileId = record.Id;
                //滑动更新文件的创建时间
                await _adoExecuterRepository.ExecuteAsync("UPDATE [FILE] SET CreateTime = GETDATE() WHERE Id = @Id", new { record.Id });
            }

            resp.Exist = false;
            resp.FileId = 0;

            return resp;
        }

        public async Task<FileInfo> GetDownloadFileInfoAsync(long userId, string messageId)
        {
            var record =
               await _adoQuerierRepository.QueryFirstOrDefaultAsync<dynamic>("SELECT * FROM ChatRecord INNER JOIN [File] ON [File].id = ChatRecord.FileId WHERE MessageId = @MessageId",
               new { MessageId = messageId });

            if (record == null || (record.From != userId && record.To != userId))
            {
                throw new Exception("下载文件错误");
            }

            if (!File.Exists(record.Location))
            {
                throw new Exception("文件不存在");
            }

            return new FileInfo(record.Location);
        }

        //https://learn.microsoft.com/zh-cn/aspnet/core/mvc/models/file-uploads?view=aspnetcore-7.0 TODO文件签名认证
        public async Task<UploadFileResponseDto> UploadFileAsync(long userId, long receiver, string code, IFormFile file)
        {
            if (file.Length > _storageOptions.MaxSize)
            {
                return null;
            }
            var sha256code = file.OpenReadStream().ToSHA256();
            if (sha256code != code)
            {
                return null;
            }
            var fileExtension = Path.GetExtension(file.FileName);
            var savepath = Path.Combine(_storageOptions.StorageLocation, DateTime.Now.ToString("yyyy-MM"), $"user-{userId}");
            var fileName = $"{Path.GetRandomFileName()}{fileExtension}";
            var fullName = Path.Combine(savepath, fileName);

            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }

            using (var fs = File.Create(fullName))
            {
                await file.CopyToAsync(fs);
                await fs.FlushAsync();
            }
            var sql = "INSERT INTO [File](CreateBy,FileName,[Size],Code,Location,Receiver,createTime) VALUES(@CreateBy,@FileName,@Size,@Code,@Location,@Receiver,GETDATE());SELECT CAST(SCOPE_IDENTITY() as bigint)";
            var fileId = await _adoQuerierRepository.QueryFirstAsync<long>(sql, new
            {
                CreateBy = userId,
                file.FileName,
                Size = file.Length,
                Code = code,
                Location = fullName,
                Receiver = receiver
            });

            return new UploadFileResponseDto() { FileId = fileId };
        }
    }
}

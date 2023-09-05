using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Shared.Domain.Metadata;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("{code}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> CheckFileExist(string code)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var resp = await _fileService.CheckFileExistInServerAsync(code, userId);

            return Ok(resp);
        }

        [HttpGet("download/{messageId}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> DownLoad(string messageId)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var info = await _fileService.GetDownloadFileInfoAsync(userId, messageId);
            if (info == null) 
            {
                return BadRequest("文件下载出错");
            }

            return new FileStreamResult(new FileStream(info.FullName, FileMode.Open), "application/octet-stream")
                { FileDownloadName = info.Name };
        }

        [HttpPost("{isGroup}/{receiver}/{code}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, bool isGroup, long receiver, string code)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var resp = await _fileService.UploadFileAsync(isGroup,userId, receiver, code, file);

            return Ok(resp);
        }
    }
}

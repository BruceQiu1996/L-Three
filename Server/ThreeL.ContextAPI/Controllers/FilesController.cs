using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.Domain.Metadata;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger _logger;
        public FilesController(IFileService fileService, ILoggerFactory loggerFactory)
        {
            _fileService = fileService;
            _logger = loggerFactory.CreateLogger(nameof(Module.CONTEXT_API_SERVICE));
        }

        [HttpGet("{code}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> CheckFileExist(string code)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var resp = await _fileService.CheckFileExistInServerAsync(code, userId);

                return Ok(resp);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem(ex.ToString());
            }
        }

        [HttpGet("download/{messageId}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> DownLoad(string messageId)
        {
            try
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem(ex.ToString());
            }
        }

        [HttpPost("{isGroup}/{receiver}/{code}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, bool isGroup, long receiver, string code)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var resp = await _fileService.UploadFileAsync(isGroup, userId, receiver, code, file);

                return Ok(resp);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem(ex.ToString());
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.Domain.Metadata;
using ThreeL.Shared.WebApi.Extensions;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public UserController(IUserService userService, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(Module.CONTEXT_API_SERVICE));
            _userService = userService;
        }

        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpPost]
        public async Task<ActionResult> Create(UserCreationDto creationDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var sresult = await _userService.CreateUserAsync(creationDto, userId);

                return sresult.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto creationDto)
        {
            try
            {
                var resp = await _userService.LoginAsync(creationDto);
                if (resp == null)
                    return NotFound();

                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpPost("login/code")]
        public async Task<ActionResult> LoginByCode(UserLoginDto creationDto)
        {
            try
            {
                var result = await _userService.LoginByCodeAsync(creationDto);
                if (!result)
                    return NotFound();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh/token")]
        public async Task<ActionResult> RefreshToken(UserRefreshTokenDto tokenDto)
        {
            try
            {
                var resp = await _userService.RefreshAuthTokenAsync(tokenDto);
                if (resp == null)
                    return BadRequest();

                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet("search/{key}")]
        public async Task<ActionResult> SearchUserByKeyword(string key)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var resp = await _userService.FindUserByKeyword(userId, key);

                return resp.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpGet("avatar/{code}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> CheckAvatarExist(string code)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var resp = await _userService.CheckAvatarExistInServerAsync(code, userId);

                return resp.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("upload/avatar/{code}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file, string code)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var resp = await _userService.UploadUserAvatarAsync(userId, code, file);
                if (resp.Value == null)
                {
                    return resp.ToActionResult();
                }

                return new FileStreamResult(new FileStream(resp.Value.FullName, FileMode.Open), "application/octet-stream") { FileDownloadName = resp.Value.Name };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpGet("download/avatar/{userId}/{avatarId}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> DownloadAvatar(long userId, long avatarId)
        {
            try
            {
                var resp = await _userService.DownloadUserAvatarAsync(avatarId, userId);
                if (resp.Value == null)
                {
                    return resp.ToActionResult();
                }

                return new FileStreamResult(new FileStream(resp.Value.FullName, FileMode.Open), "application/octet-stream") { FileDownloadName = resp.Value.Name };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("group/{groupName}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> CreateGroup(string groupName)
        {
            try
            {
                if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length <= 2)
                    return BadRequest("非法群名");

                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var resp = await _userService.CreateGroupChatAsync(userId, groupName);

                return resp.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpGet("{fuserId}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> FetchUserDetail(long fuserId)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var resp = await _userService.FetchUserInfoByIdAsync(userId, fuserId);

                return resp.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpGet("group/{groupId}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> FetchGroupDetail(long groupId)
        {
            try
            {
                var resp = await _userService.FetchGroupInfoByIdAsync(groupId);

                return resp.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }
    }
}

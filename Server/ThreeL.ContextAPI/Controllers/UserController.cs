using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Shared.Domain.Metadata;
using ThreeL.Shared.WebApi.Extensions;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        //[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpPost]
        public async Task<ActionResult> Create(UserCreationDto creationDto)
        {
            var sresult = await _userService.CreateUserAsync(creationDto, 0);

            return sresult.ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto creationDto)
        {
            var resp = await _userService.LoginAsync(creationDto);
            if (resp == null)
                return NotFound();

            return Ok(resp);
        }

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpPost("login/code")]
        public async Task<ActionResult> LoginByCode(UserLoginDto creationDto)
        {
            var result = await _userService.LoginByCodeAsync(creationDto);
            if (!result)
                return NotFound();

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("refresh/token")]
        public async Task<ActionResult> RefreshToken(UserRefreshTokenDto tokenDto)
        {
            var resp = await _userService.RefreshAuthTokenAsync(tokenDto);
            if (resp == null)
                return BadRequest();

            return Ok(resp);
        }

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet("search/{key}")]
        public async Task<ActionResult> SearchUserByKeyword(string key)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var resp = await _userService.FindUserByKeyword(userId, key);

            return resp.ToActionResult();
        }

        [HttpGet("avatar/{code}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> CheckAvatarExist(string code)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var resp = await _userService.CheckAvatarExistInServerAsync(code, userId);

            return resp.ToActionResult();
        }

        [HttpPost("upload/avatar/{code}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file, string code)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var resp = await _userService.UploadUserAvatarAsync(userId, code, file);
            if (resp.Value == null)
            {
                return resp.ToActionResult();
            }

            return new FileStreamResult(new FileStream(resp.Value.FullName, FileMode.Open), "application/octet-stream") { FileDownloadName = resp.Value.Name };
        }

        [HttpGet("download/avatar/{userId}/{avatarId}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> DownloadAvatar(long userId, long avatarId)
        {
            var resp = await _userService.DownloadUserAvatarAsync(avatarId, userId);
            if (resp.Value == null)
            {
                return resp.ToActionResult();
            }

            return new FileStreamResult(new FileStream(resp.Value.FullName, FileMode.Open), "application/octet-stream") { FileDownloadName = resp.Value.Name };
        }

        [HttpPost("group/{groupName}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> CreateGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length <= 2)
                return BadRequest("非法群名");

            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var resp = await _userService.CreateGroupChatAsync(userId, groupName);

            return resp.ToActionResult();
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        public async Task<IActionResult> FetchUserDetail(long fuserId)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var resp = await _userService.FetchUserInfoByIdAsync(userId, fuserId);

            return resp.ToActionResult();
        }
    }
}

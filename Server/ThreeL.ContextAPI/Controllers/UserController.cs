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

        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
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
    }
}

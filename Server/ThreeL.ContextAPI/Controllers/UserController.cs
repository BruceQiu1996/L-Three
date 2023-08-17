using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeL.ContextAPI.Application.Contract.Dtos.User;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Shared.Domain.Metadata;

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
            await _userService.CreateUserAsync(creationDto, 0); //TODO使用用户id
            return Ok();
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
    }
}

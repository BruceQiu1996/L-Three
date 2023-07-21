using Microsoft.AspNetCore.Mvc;
using ThreeL.ContextAPI.Application.Contract.Services;

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

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _userService.SearchAllUsersAsync());
        }
    }
}

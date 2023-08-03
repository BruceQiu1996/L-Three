using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Shared.Domain.Metadata;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationsController : ControllerBase
    {
        private readonly IFriendService _friendService;
        public RelationsController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        //[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet("friends")]
        public async Task<ActionResult> GetFriends()
        {
           //var result = long.TryParse(HttpContext.User.Identity?.Name,out var userId);
           // if (!result)
           //     return Unauthorized();

            var friends = await _friendService.GetFriendsAsync(2);
            return Ok(friends);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using ThreeL.ContextAPI.Application.Contract.Services;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmojisController : ControllerBase
    {
        private readonly IEmojiService _emojiService;
        public EmojisController(IEmojiService emojiServic)
        {
            _emojiService = emojiServic;
        }

        //[Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet]
        public async Task<ActionResult> GetEmojis()
        {
            var resp = await _emojiService.GetEmojiGroupsAsync(HttpContext.Request.Host.Value, 
                AppDomain.CurrentDomain.BaseDirectory);

            return Ok(resp);
        }
    }
}

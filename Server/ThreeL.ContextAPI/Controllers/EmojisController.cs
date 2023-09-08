using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.Domain.Metadata;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmojisController : ControllerBase
    {
        private readonly IEmojiService _emojiService;
        private readonly ILogger _logger;
        public EmojisController(IEmojiService emojiServic, ILoggerFactory loggerFactory)
        {
            _emojiService = emojiServic;
            _logger = loggerFactory.CreateLogger(nameof(Module.CONTEXT_API_SERVICE));
        }

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet]
        public async Task<ActionResult> GetEmojis()
        {
            try
            {
                var resp = await _emojiService.GetEmojiGroupsAsync(HttpContext.Request.Host.Value,
                AppDomain.CurrentDomain.BaseDirectory);

                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem(ex.ToString());
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.Domain.Metadata;
using ThreeL.Shared.WebApi.Extensions;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationsController : ControllerBase
    {
        private readonly IRelationService _relationService;
        private readonly ILogger logger;
        public RelationsController(IRelationService relationService,ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger(nameof(Module.CONTEXT_API_SERVICE));
            _relationService = relationService;
        }

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet("{time}")]
        public async Task<ActionResult> GetRelationsAndChatRecords(DateTime time)
        {
            try
            {
                var result = long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                if (!result)
                    return Unauthorized();

                var friends = await _relationService.GetRelationsAndChatRecordsAsync(userId, time);
                return Ok(friends);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return Problem(ex.ToString());
            }
        }

        //[Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        //[HttpGet("{friendId}/{time}/chatRecords")]
        //public async Task<ActionResult> FetchRecentlyChatRecords(long friendId, DateTime time)
        //{
        //    long.TryParse(HttpContext.User.Identity?.Name, out var userId);
        //    //验证两个人是否是好友
        //    if (friendId != userId)
        //    {
        //        var result = await _friendService.IsFriendAsync(userId, friendId);
        //        if (!result)
        //            return BadRequest();
        //    }

        //    var records = await _friendService.FetchChatRecordsWithFriendAsync(userId, friendId, time);

        //    return Ok(records);
        //}

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet("applys")]
        public async Task<ActionResult> FetchFriendApplys()
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                //验证两个人是否是好友
                var applys = await _relationService.FetchAllFriendApplysAsync(userId);

                return Ok(applys);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return Problem(ex.ToString());
            }
        }

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet("chatRecords/{relationId}/{isGroup}/{time}")]
        public async Task<ActionResult> GetChatRecords(long relationId, bool isGroup, DateTime time)
        {
            try
            {
                var result = long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                if (!result)
                    return Unauthorized();

                var records = await _relationService.GetChatRecordsByUserIdAsync(userId, relationId, isGroup, time);

                return records.ToActionResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return Problem(ex.ToString());
            }
        }
    }
}

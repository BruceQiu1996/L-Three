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
        private readonly IRelationService _relationService;
        public RelationsController(IRelationService relationService)
        {
            _relationService = relationService;
        }

        [Authorize(Roles = $"{nameof(Role.User)},{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet("{time}")]
        public async Task<ActionResult> GetRelationsAndChatRecords(DateTime time)
        {
            var result = long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            if (!result)
                return Unauthorized();

            var friends = await _relationService.GetRelationsAndChatRecordsAsync(1, time);
            return Ok(friends);
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
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            //验证两个人是否是好友
            var applys = await _relationService.FetchAllFriendApplysAsync(userId);

            return Ok(applys);
        }
    }
}

﻿using ThreeL.ContextAPI.Application.Contract.Dtos.Relation;
using ThreeL.Shared.Application.Contract.Services;

namespace ThreeL.ContextAPI.Application.Contract.Services
{
    public interface IRelationService : IAppService
    {
        Task<IEnumerable<RelationDto>> GetRelationsAndChatRecordsAsync(long userId, DateTime dateTime);
        Task<ServiceResult<IEnumerable<ChatRecordResponseDto>>> GetChatRecordsByUserIdAsync(long userId, long relationId, bool isGroup, DateTime dateTime);
        Task<bool> IsFriendAsync(long userId, long fUserId);
        Task<IEnumerable<FriendApplyResponseDto>> FetchAllFriendApplysAsync(long userId);
    }
}

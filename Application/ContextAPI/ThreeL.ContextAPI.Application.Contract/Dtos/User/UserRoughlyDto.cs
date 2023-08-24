﻿using ThreeL.Shared.Domain.Metadata;

namespace ThreeL.ContextAPI.Application.Contract.Dtos.User
{
    public class UserRoughlyDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public long? Avatar { get; set; }
        public string Sign { get; set; }
        public bool IsFriend { get; set; } //已经建立好友关系
        public string Role { get; set; }
    }
}
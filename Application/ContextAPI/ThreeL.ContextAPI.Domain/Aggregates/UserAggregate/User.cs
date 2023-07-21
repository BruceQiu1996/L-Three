﻿using ThreeL.Infra.Repository.Entities;
using ThreeL.Shared.Domain.Entities;

namespace ThreeL.ContextAPI.Domain.Aggregates.UserAggregate
{
    public class User : AggregateRoot<long>, ISoftDelete, IBasicAuditInfo<long>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public string Sign { get; set; }
        public bool IsDeleted { get; set; }
        public long CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
    }
}

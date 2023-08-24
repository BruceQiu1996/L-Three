using System.ComponentModel;

namespace ThreeL.Shared.Domain.Metadata
{
    public enum Role
    {
        [Description("用户")]
        User,
        [Description("管理员")]
        Admin,
        [Description("超级管理员")]
        SuperAdmin
    }
}

using ThreeL.Shared.Domain.Metadata;

namespace ThreeL.ContextAPI.Application.Contract.Dtos.User
{
    public class UserCreationDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public Role? Role { get; set; }
    }
}

namespace ThreeL.ContextAPI.Application.Contract.Dtos.User
{
    public class UserAvatarCheckExistResponseDto
    {
        public bool Exist { get; set; }
        public byte[] Avatar { get; set; }
    }
}

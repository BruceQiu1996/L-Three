namespace ThreeL.ContextAPI.Application.Contract.Dtos.User
{
    public class UserLoginResponseDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public byte[] Avatar { get; set; }
        public long? AvatarId { get; set; }
    }
}

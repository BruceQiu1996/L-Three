namespace ThreeL.Client.Shared.Entities
{
    public class UserProfile
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string SocketAccessToken { get; set; }
        public string Role { get; set; }
        public DateTime LastLoginTime { get; set; }
    }
}

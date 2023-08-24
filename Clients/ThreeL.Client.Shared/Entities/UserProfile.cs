namespace ThreeL.Client.Shared.Entities
{
    public class UserProfile
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string ShowName => UserName?.Substring(0, 1)?.ToUpper();
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string SocketAccessToken { get; set; }
        public string Role { get; set; }
        public DateTime LastLoginTime { get; set; }
        public long? AvatarId { get; set; }
        public byte[] Avatar { get; set; }

        public void Clear()
        {
            UserId = 0;
            UserName = null;
            AccessToken = null;
            RefreshToken = null;
            SocketAccessToken = null;
            Role = null;
        }
    }
}

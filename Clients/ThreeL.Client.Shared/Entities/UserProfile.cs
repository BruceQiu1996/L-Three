namespace ThreeL.Client.Shared.Entities
{
    public class UserProfile
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}

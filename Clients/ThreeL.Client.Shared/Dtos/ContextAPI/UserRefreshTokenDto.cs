﻿namespace ThreeL.Client.Shared.Dtos.ContextAPI
{
    public class UserRefreshTokenDto
    {
        public string Origin { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}

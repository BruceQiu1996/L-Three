﻿namespace ThreeL.ContextAPI.Application.Contract.Dtos.User
{
    public class UserLoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}

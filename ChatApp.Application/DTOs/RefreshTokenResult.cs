using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.DTOs
{
    public class RefreshTokenResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public static RefreshTokenResult Success(string accessToken, string refreshtoken)
            => new() { IsSuccess = true, AccessToken = accessToken, RefreshToken = refreshtoken };

        public static RefreshTokenResult Fail(string message)
            => new() { IsSuccess = false, ErrorMessage = message };

    }
}

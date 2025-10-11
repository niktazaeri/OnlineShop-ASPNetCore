using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace WebApplication1_API_MVC_.Identity
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string UserId { get; set; }
        public bool IsRevoked { get; set; }
        public ApplicationUser User { get; set; }

        public static RefreshToken GenerateRefreshToken(ApplicationUser user)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var token = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            return token;

        }
    }
}

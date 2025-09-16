using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MiniCRM.Api.Models;

namespace MiniCRM.Api.Services
{
    public class JwtService
    {
        private readonly string _key;

        public JwtService(IConfiguration config)
        {
            _key = config["Jwt:Key"] ?? throw new Exception("JWT key not found");
            Console.WriteLine("JWT Key: " + _key); // Debug amaçlý log
        }


        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "MiniCRM",
                audience: "MiniCRMClient",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

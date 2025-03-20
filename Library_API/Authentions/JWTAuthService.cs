using BookStore_API.DataAccess;
using BookStore_API.Models;
using Google;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookStore_API.Authentions
{
    public class JWTAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly BookStoreContext _context;
        private readonly SymmetricSecurityKey _securityKey;

        public JWTAuthService(IConfiguration configuration, BookStoreContext context)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            // Cache SymmetricSecurityKey để tránh tạo lại nhiều lần
            string jwtKey = _configuration["JwtSettings:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }

        public string GenerateJwtToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? throw new ArgumentException("Username cannot be null.", nameof(user.Username))),
                new Claim(ClaimTypes.MobilePhone, user.Phone?? throw new ArgumentException("Phone cannot be null.", nameof(user.Phone))),
                new Claim(ClaimTypes.Email, user.Email?? throw new ArgumentException("Email cannot be null.", nameof(user.Email))),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured."),
                audience: _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured."),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetExpireMinutes()), // Sử dụng UTC để tránh vấn đề múi giờ
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetExpireMinutes()
        {
            return int.TryParse(_configuration["JwtSettings:ExpireMinutes"], out int minutes) ? minutes : 60;
        }

    }
}

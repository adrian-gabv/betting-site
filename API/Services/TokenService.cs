using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService(UserManager<AppUser> userManager, IOptions<JwtSettings> jwtSettings) : ITokenService
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(
                jwtSettings.Value.TokenKey ?? throw new InvalidOperationException("TokenKey is not configured")));
        private readonly UserManager<AppUser> _userManager = userManager;

        public async Task<string> CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName!)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim("role", role)));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JsonWebTokenHandler();

            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}

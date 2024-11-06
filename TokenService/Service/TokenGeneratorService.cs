using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TokenService.Controllers;

namespace TokenService.Service;

public static class TokenGeneratorService
{
    public static string GenerateToken(string id, string email, List<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var secretKey = "92336b63be8446e9a9ea351da752bd1a";

        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, id),
            new(JwtRegisteredClaimNames.Email, email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60), //TODO change to 10-15 minutes later
            Issuer = "https://www.rika.com",
            Audience = "https://www.rika.com",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
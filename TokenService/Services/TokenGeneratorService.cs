using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TokenService.Models.ResponseModels;

namespace TokenService.Services;

public static class TokenGeneratorService
{
	public static string GenerateToken(ResponseContent content, string secretKey)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(secretKey);

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new(JwtRegisteredClaimNames.Sub, content.Id),
			new(JwtRegisteredClaimNames.Email, content.Email)
		};

		claims.AddRange(content.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
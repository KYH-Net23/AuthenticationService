using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TokenService.Models.ResponseModels;

namespace TokenService.Services;

public static class TokenGeneratorService
{
    public static string GenerateAccessToken(Content content, string secretKey, int expirationMinutes)
    {
        var key = Encoding.ASCII.GetBytes(secretKey);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, content.Email),
            new(JwtRegisteredClaimNames.Email, content.Email),
            new(JwtRegisteredClaimNames.Name, content.Email)
        };
        claims.AddRange(content.Role.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = "https://www.rika.com",
            Audience = "https://www.rika.com",
            Claims = claims.ToDictionary(claim => claim.Type, object (claim) => claim.Value),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
    }
    
    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, string secretKey)
    {
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals
                (SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        
        return principal;
    }

    public static string GenerateAccessTokenToEmailProvider(string providerName, string secretKey, int expirationMinutes)
    {
        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new Dictionary<string, object>
        {
            { "provider", providerName.Split('-').First() }
        };

        var claimsIdentity = new ClaimsIdentity([new Claim(providerName.Split('-').First(), providerName.Split('-').First())]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = "https://www.rika.com",
            Audience = "https://www.rika.com",
            Claims = claims,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
    }

    public static bool ValidateToken(string token, string secretKey)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TokenService.Context;
using TokenService.Models.DataModels;
using TokenService.Models.ResponseModels;
using TokenService.Services;

namespace TokenService.Controllers;

[ApiController]
public class AuthController(IConfiguration config, DataContext context, IOptions<CookieSettings> cookieSettings, IOptions<TokenSettings> tokenSettings) : ControllerBase
{
    private readonly string _secretKey = config["TokenServiceSecretAccessKey"]!;
    private readonly int _accessTokenCookieDurationInHours = cookieSettings.Value.AccessTokenCookieDurationInHours;
    private readonly int _accessTokenDurationInMinutes = tokenSettings.Value.AccessTokenDurationInMinutes;

    [HttpGet("authorize")]
    [Authorize]
    public IActionResult Validate()
    {
        try
        {
            var isAuthenticated = User.Identity!.IsAuthenticated;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                isAuthenticated,
                role
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during validation", error = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        try
        {
            Response.Cookies.Append("accessToken", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(-1)
            });
            return Ok(new { message = "Successfully logged out" });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during logout", error = ex.Message });
        }
    }
    
    [HttpPost]
    [Route("refresh")]
    public IActionResult Refresh()
    {
        try
        {
            if (!Request.Cookies.TryGetValue("accessToken", out var accessToken) || !Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized();
            }

            var principal = TokenGeneratorService.GetPrincipalFromExpiredToken(accessToken, _secretKey);

            var nameClaim = principal.FindFirst("name")?.Value;
            if (nameClaim is null)
            {
                return Unauthorized();
            }

            var username = principal.Claims.First(n => n.Type == "name").Value;
            var userRefreshToken = context.Users.FirstOrDefault(u => u.UserName == username && !u.IsRevoked);

            if (userRefreshToken == null || userRefreshToken.RefreshToken != refreshToken || userRefreshToken.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest(new {Message ="Invalid refresh token or token expired. Please login again."});

            var responseContent = new ResponseContent
            {
                Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                Roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
            };

            var newAccessToken = TokenGeneratorService.GenerateAccessToken(responseContent, _secretKey, _accessTokenDurationInMinutes);

            Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(_accessTokenCookieDurationInHours)
            });

            return Ok(new
            {
                Message = "Token refreshed successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during refresh", error = ex.Message });
        }
    }

    // [HttpPost, Authorize]
    // [Route("revoke")]
    // public IActionResult Revoke()
    // {
    //     var username = User.Identity.Name;
    //     var user = _userContext.LoginModels.SingleOrDefault(u => u.UserName == username);
    //     if (user == null) return BadRequest();
    //     user.RefreshToken = null;
    //     _userContext.SaveChanges();
    //     return NoContent();
    // }
}
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
public class AuthController : ControllerBase
{
    private readonly string _secretKey;
    private readonly int _accessTokenCookieDurationInHours;
    private readonly int _accessTokenDurationInMinutes;
    private readonly DataContext _context;

    public AuthController(IConfiguration config, DataContext context, IOptions<CookieSettings> cookieSettings, IOptions<TokenSettings> tokenSettings)
    {
        _context = context;
        _secretKey = config["TokenServiceSecretAccessKey"]!;
        _accessTokenCookieDurationInHours = cookieSettings.Value.AccessTokenCookieDurationInHours;
        _accessTokenDurationInMinutes = tokenSettings.Value.AccessTokenDurationInMinutes;
    }

    [HttpGet("authorize")]
    [Authorize]
    public IActionResult Validate()
    {
        if (!Request.Cookies.TryGetValue("accessToken", out _) || !Request.Cookies.TryGetValue("refreshToken", out _))
        {
            return Unauthorized(new {isAuthenticated = false, role = Array.Empty<string>()});
        }
        
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
            var refreshToken = _context.Tokens.FirstOrDefault(u => u.Token == Request.Cookies["refreshToken"]);
            if (refreshToken == null) return
                StatusCode(StatusCodes.Status418ImATeapot, new { message = "Unclear error. Contact the administrator" });

            refreshToken.IsRevoked = true;
            _context.SaveChanges();
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
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
            var userRefreshToken = _context.Tokens.FirstOrDefault(u => u.UserName == username && !u.IsRevoked);

            if (userRefreshToken == null || userRefreshToken.Token != refreshToken || userRefreshToken.TokenExpiryTime <= DateTime.UtcNow)
                return BadRequest(new {Message ="Invalid refresh token or token expired. Please login again."});

            var responseContent = new Content
            {
                Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                Role = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToList()
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
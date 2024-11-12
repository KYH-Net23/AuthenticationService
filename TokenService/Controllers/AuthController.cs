using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using TokenService.Context;
using TokenService.Models.ResponseModels;
using TokenService.Services;

namespace TokenService.Controllers;

[ApiController]
public class AuthController(IConfiguration config, DataContext context) : ControllerBase
{
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
            if (!Request.Cookies.TryGetValue("accessToken", out var accessToken) ||
                !Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return BadRequest("Invalid client request: Missing tokens in headers");
            }
            // var accessToken = Request.Cookies["accessToken"]!.ToString().Replace("Bearer", string.Empty);
            // var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Invalid refresh token" });

            var secretKey = config["TokenServiceSecretAccessKey"];

            var principal = TokenGeneratorService.GetPrincipalFromExpiredToken(accessToken, secretKey!);

            // Validate the refresh token in the database
            var username = principal.Claims.First(n => n.Type == "name").Value; // Use Name claim for username
            var user = context.RefreshTokens.SingleOrDefault(u => u.UserName == username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest("Invalid refresh token or token expired");

            // Generate new tokens
            var responseContent = new ResponseContent
            {
                Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                Roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
            };

            var newAccessToken = TokenGeneratorService.GenerateAccessToken(responseContent, secretKey!);
            var newRefreshToken = TokenGeneratorService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            context.SaveChanges();

            // Send the new tokens in response
            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) // Adjust expiration as needed
            });

            Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(5) // Adjust expiration as needed
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
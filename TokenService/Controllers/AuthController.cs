using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TokenService.Controllers;

[ApiController]
public class AuthController : ControllerBase
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
            Response.Cookies.Delete("accessToken");
            return Ok(new { message = "Successfully logged out" });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during logout", error = ex.Message });
        }
    }
}
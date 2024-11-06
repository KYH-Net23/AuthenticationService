using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TokenService.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    [HttpGet("authorize")]
    [Authorize]
    public async Task<IActionResult> Validate()
    {
        var isAuthenticated = User.Identity.IsAuthenticated;
        var role = User.FindAll(ClaimTypes.Role).Select(c => c.Value).First();

        return Ok(new
        {
            isAuthenticated,
            role
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("token");
        return Ok();
    }
}
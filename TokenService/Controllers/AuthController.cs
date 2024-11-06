using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TokenService.Controllers;

[Route("authorize")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpGet]
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
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokenService.Service;

namespace TokenService.Controllers;

[ApiController]
[Route("[controller]")]
public class TokenGeneratorController(TokenGeneratorService tokenGeneratorService) : ControllerBase
{
    [HttpGet]
    public string GetToken(string email)
    {
        return tokenGeneratorService.GenerateToken(email);
    }
}
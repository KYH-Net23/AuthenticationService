using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using TokenService.Service;

namespace TokenService.Controllers;

[ApiController]
[Route("[controller]")]
public class TokenGeneratorController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        // call on identity login endpoint

        var identityLoginUrl = "https://rika-identity-user-f5e3fddxg4bve2eg.swedencentral-01.azurewebsites.net/";

        var options = new RestClientOptions(identityLoginUrl);
        var client = new RestClient(options);

        var request = new RestRequest("api/CustomerLogin/login", Method.Post)
        {
            RequestFormat = DataFormat.Json
        };

        request.AddHeader("accept", "text/plain");
        request.AddHeader("Content-Type", "application/json");

        request.AddJsonBody(new
        {
            email = loginRequest.Email,
            password = loginRequest.Password
        });

        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            var token = TokenGeneratorService.GenerateToken(loginRequest.Email);
            return Ok(token);
        }

        return Unauthorized();
    }

    [HttpGet]
    public string GetToken(string email)
    {
        // test login
        // call on login service from identity api
        // if true generate token
        return TokenGeneratorService.GenerateToken(email);

        // else
        // error
    }
}
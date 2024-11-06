using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using TokenService.Models;
using TokenService.Models.FormModels;
using TokenService.Models.ResponseModels;
using TokenService.Services;

namespace TokenService.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("[controller]")]
public class TokenGeneratorController(IOptions<ApiSettings> apiSettings) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginRequest)
    {
        var identityLoginUrl = new Uri(apiSettings.Value.BaseUrl);

        var options = new RestClientOptions(identityLoginUrl);
        var client = new RestClient(options);

        var request = new RestRequest("/login", Method.Post)
        {
            RequestFormat = DataFormat.Json
        };

        request.AddHeader("accept", "text/plain");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(loginRequest);

        var response = await client.ExecuteAsync(request);

        var result = JsonConvert.DeserializeObject<ResponseResult>(response.Content!);

        if (!response.IsSuccessful) return Unauthorized();
        if(result == null) return BadRequest();

        var token = TokenGeneratorService.GenerateToken(result.ResponseContent);
        return Ok(new {token});

    }
}
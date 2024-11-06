using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using TokenService.Service;

namespace TokenService.Controllers;

[ApiController]
[Route("[controller]")]
public class TokenGeneratorController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginRequest)
    {
        var identityLoginUrl = "https://rika-identity-user-f5e3fddxg4bve2eg.swedencentral-01.azurewebsites.net";

        var options = new RestClientOptions(identityLoginUrl);
        var client = new RestClient(options);

        var request = new RestRequest("api/CustomerLogin/login", Method.Post) // TODO change to a single login endpoint
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

        var deserializedResponse = JsonConvert.DeserializeObject<Root>(response.Content);

        if (response.IsSuccessful)
        {
            var token = TokenGeneratorService.GenerateToken(deserializedResponse.Content.Id, deserializedResponse.Content.Email, deserializedResponse.Content.Roles);
            return Ok(new {token});
        }

        return Unauthorized();
    }
}

public class LoginModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class Content
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonProperty("roles")]
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; }
}

public class Root  // TODO change name later
{
    [JsonProperty("message")]
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonProperty("content")]
    [JsonPropertyName("content")]
    public Content Content { get; set; }
}

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
public class TokenGeneratorController(IOptions<ApiSettings> apiSettings, IConfiguration configuration
	) : ControllerBase
{
	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginModel loginRequest)
	{
		if (!ModelState.IsValid)
			return BadRequest(loginRequest);

		var client = new RestClient(new RestClientOptions(new Uri(apiSettings.Value.BaseUrl)));
		var request = new RestRequest("/login", Method.Post)
			.AddHeader("accept", "text/plain")
			.AddHeader("Content-Type", "application/json")
			.AddJsonBody(loginRequest);

		var response = await client.ExecuteAsync(request);
		var result = JsonConvert.DeserializeObject<ResponseResult>(response.Content!);

		if (!response.IsSuccessful) return Unauthorized();
		if (result == null) return BadRequest();

		var token = TokenGeneratorService.GenerateToken(result.ResponseContent, configuration["TokenServiceSecretAccessKey"]!);
		Response.Cookies.Append("accessToken", token, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.None,
			Expires = DateTime.UtcNow.AddHours(1),
			Path = "/"
		});

		return Ok(new { Message = "Success!" });
	}
}
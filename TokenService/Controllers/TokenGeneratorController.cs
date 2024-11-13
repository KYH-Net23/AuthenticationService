using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using TokenService.Context;
using TokenService.Models.DataModels;
using TokenService.Models.FormModels;
using TokenService.Models.ResponseModels;
using TokenService.Services;

namespace TokenService.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("[controller]")]
public class TokenGeneratorController(IOptions<ApiSettings> apiSettings, IConfiguration configuration, DataContext context,
	IOptions<CookieSettings> cookieSettings, IOptions<TokenSettings> tokenSettings) : ControllerBase
{

	private readonly string _secretKey = configuration["TokenServiceSecretAccessKey"]!;
	private readonly int _accessTokenCookieDurationInHours = cookieSettings.Value.AccessTokenCookieDurationInHours;
	private readonly int _refreshTokenCookieDurationInHours = cookieSettings.Value.RefreshTokenCookieDurationInHours;
	private readonly int _accessTokenDurationInMinutes = tokenSettings.Value.AccessTokenDurationInMinutes;

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
		var result = JsonConvert.DeserializeObject<ResponseResult>(response.Content!);  // why is name null here?

		if (!response.IsSuccessful) return Unauthorized();
		if (result == null) return BadRequest();

		var token = TokenGeneratorService.GenerateAccessToken(result.ResponseContent, _secretKey, _accessTokenDurationInMinutes);
		var refreshToken = TokenGeneratorService.GenerateRefreshToken();

		Response.Cookies.Append("accessToken", token, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.None,
			Expires = DateTime.UtcNow.AddHours(_accessTokenCookieDurationInHours),
			Path = "/"
		});

		Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.None,
			Expires = DateTime.UtcNow.AddHours(_refreshTokenCookieDurationInHours),
			Path = "/"
		});
		
		var refreshTokenModel = new User
		{
			UserName = result.ResponseContent.Email,
			RefreshToken = refreshToken,
			RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(_refreshTokenCookieDurationInHours)
		};

		var user = await context.Users.FirstOrDefaultAsync(x => x.UserName == result.ResponseContent.Email);

		if (user == null)
		{
			context.Users.Add(new User
			{
				UserName = result.ResponseContent.Email,
				RefreshToken = refreshToken,
				RefreshTokenExpiryTime = refreshTokenModel.RefreshTokenExpiryTime,
				IsRevoked = false
			});
		}
		else
		{
			user.RefreshToken = refreshTokenModel.RefreshToken;
			user.RefreshTokenExpiryTime = refreshTokenModel.RefreshTokenExpiryTime;
			context.Users.Update(user);
		}

		await context.SaveChangesAsync();

		return Ok(new { Message = "Success!" });
	}
}
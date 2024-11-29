using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TokenService.Context;
using TokenService.Extensions;
using TokenService.Models.DataModels;
using TokenService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.Configure<CookieSettings>(builder.Configuration.GetSection("CookieSettings"));
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.AddDbContext<DataContext>(o => o.UseSqlServer(builder.Configuration["TokenDataConnectionString"]!));

var vaultUri = new Uri($"{builder.Configuration["VaultUrl"]!}");
if (builder.Environment.IsDevelopment())
{
	builder.Configuration.AddAzureKeyVault(
		vaultUri,
		new VisualStudioCredential());
}
else
{
	builder.Configuration.AddAzureKeyVault(
		vaultUri,
		new DefaultAzureCredential());
}

var secretKey = builder.Configuration["TokenServiceSecretAccessKey"];

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigin",
		policy => policy.WithOrigins("http://localhost:5173", "https://localhost:7076")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials());
});

builder.Services.AddSingleton<KeyVaultService>(_ => new KeyVaultService(vaultUri));

// builder.Services.AddEndpointsApiExplorer();

// Custom extension methods
builder.Services.AddAuthenticationExtension(secretKey);
builder.Services.AddCustomRateLimiter();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
	options.WithTheme(ScalarTheme.Mars)
		.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
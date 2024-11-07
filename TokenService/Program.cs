using Azure.Identity;
using TokenService.Extensions;
using TokenService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// builder.Services.AddDbContext<DataContext>(o => o.UseMySQL(builder.Configuration["IdentityServiceConnectionString"]!));

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
		policy => policy.WithOrigins("http://localhost:5173")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();

// Custom extension methods
builder.Services.AddSwaggerGenWithConfig();
builder.Services.AddAuthenticationExtension(secretKey);
builder.Services.AddCustomRateLimiter();

var app = builder.Build();

app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
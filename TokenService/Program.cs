using System.Diagnostics;
using System.Text;
using System.Threading.RateLimiting;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TokenService.Models;
using TokenService.Services;

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
if (string.IsNullOrEmpty(secretKey))
{
	Console.WriteLine("Empty");
}
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigin",
		policy => policy.WithOrigins("http://localhost:5173")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition(
		"Bearer",
		new OpenApiSecurityScheme
		{
			Description =
				"JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT"
		}
	);

	options.AddSecurityRequirement(
		new OpenApiSecurityRequirement
		{
			{
				new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = "Bearer"
					}
				},
				[]
			}
		}
	);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = "https://www.rika.com",
		ValidAudience = "https://www.rika.com",
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
	};

	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			var token = context.Request.Cookies["accessToken"];
			if (token != null)
			{
				context.Token = token;
			}

			return Task.CompletedTask;
		}
	};
});

builder.Services.AddRateLimiter(options =>
{
	options.OnRejected = async (context, token) =>
	{
		context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
		if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
		{
			await context.HttpContext.Response.WriteAsync($"Too many requests. Retry after {retryAfter.TotalMinutes} minutes.", cancellationToken: token);
		}
		else
		{
			await context.HttpContext.Response.WriteAsync($"Too many requests. Retry after {retryAfter.TotalMinutes} minutes.", cancellationToken: token);
		}
	};

	options.AddTokenBucketLimiter("token", tokenOptions =>
	{
		tokenOptions.TokenLimit = 10_000;
		tokenOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		tokenOptions.QueueLimit = 10;
		tokenOptions.ReplenishmentPeriod = TimeSpan.FromDays(1);
		tokenOptions.TokensPerPeriod = 10_000;
		tokenOptions.AutoReplenishment = true;
	});
});

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
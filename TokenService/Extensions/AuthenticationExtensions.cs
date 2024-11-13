﻿using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TokenService.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationExtension(this IServiceCollection services, string secretKey)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://www.rika.com",
            ValidAudience = "https://www.rika.com",
            IssuerSigningKey = key
        };

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["accessToken"];
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // if (context.AuthenticateFailure == null ||
                        //     context.AuthenticateFailure.GetType() != typeof(SecurityTokenExpiredException))
                        //     return Task.CompletedTask;
                        
                        var refreshToken = context.Request.Cookies["refreshToken"];
                        
                        if (string.IsNullOrEmpty(refreshToken)) return Task.CompletedTask;
                        
                        context.Response.Redirect($"/auth/refresh");
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
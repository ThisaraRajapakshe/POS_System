using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using POS_System.Configurations;

namespace POS_System.Extensions
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddPosSystemJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind JWT settings
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JWT settings not found.");

            // Clear default claim mappings
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var auth = ctx.Request.Headers["Authorization"].ToString();
                        var preview = string.IsNullOrEmpty(auth) ? "<none>" : (auth.Length <= 120 ? auth : auth.Substring(0, 120) + "...");
                        logger.LogInformation("JwtBearer.OnMessageReceived: Authorization header preview: {preview}", preview);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var claims = string.Join(", ", ctx.Principal!.Claims.Select(c => $"{c.Type}={c.Value}"));
                        logger.LogInformation("JwtBearer.OnTokenValidated: validated. Claims: {claims}", claims);
                        logger.LogInformation("JwtBearer.OnTokenValidated: IsInRole('Admin') => {isAdmin}", ctx.Principal!.IsInRole("Admin"));
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(ctx.Exception, "JwtBearer.OnAuthenticationFailed: {msg}", ctx.Exception?.Message);
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}

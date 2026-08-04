using Microsoft.OpenApi.Models;

namespace POS_System.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddPosSystemSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "POS System API",
                    Version = "v1",
                    Description = "A comprehensive Point of Sale System API",
                    Contact = new OpenApiContact { Name = "Thisara Rajapakshe", Email = "thisararajapakshe2020@gmail.com" }
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer {token}'",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                c.AddSecurityDefinition("Bearer", securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
                });
            });

            return services;
        }
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POS_System.ApplicationServices;
using POS_System.ApplicationServices.Implementation;
using POS_System.Configurations;
using POS_System.Data;
using POS_System.Mapping;
using POS_System.Models.Identity;
using POS_System.Repositories;
using POS_System.Repositories.Implementation;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Services --------------------------------------------------------------

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "POS System API",
        Version = "v1",
        Description = "A comprehensive Point of Sale System API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Thisara Rajapakshe",
            Email = "thisararajapakshe2020@gmail.com"
        }
    });

    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DB contexts
builder.Services.AddDbContext<PosSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PosSystemConnectionString")));

builder.Services.AddDbContext<PosSystemAuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PosSystemAuthConnectionString")));

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // password + lockout + user settings (same as before)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<PosSystemAuthDbContext>()
.AddDefaultTokenProviders();

// prevent cookie-redirects for APIs (return proper 401/403)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

// App services / repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductLineItemRepository, ProductLineItemRepository>();
builder.Services.AddScoped<IProductLineItemService, ProductLineItemService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfiles>());

// JWT configuration
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not found.");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// --- Authentication (JWT only) --------------------------------------------

// Make claim mapping explicit (do this BEFORE AddAuthentication)
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

// Configure JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero,

            // These must match the claim names in your generated tokens
            RoleClaimType = "role",
            NameClaimType = "nameid"
        };

        // Log token validation / failures (safe)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                // log the incoming raw token length/preview (safe)
                var token = ctx.Request.Headers["Authorization"].FirstOrDefault();
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                if (!string.IsNullOrEmpty(token))
                {
                    var preview = token.Length <= 200 ? token : token.Substring(0, 200) + "...";
                    logger.LogInformation("JwtBearer.OnMessageReceived: Authorization header preview: {preview}", preview);
                }
                else
                {
                    logger.LogInformation("JwtBearer.OnMessageReceived: Authorization header not provided");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var claims = string.Join(", ", ctx.Principal!.Claims.Select(c => $"{c.Type}={c.Value}"));
                logger.LogInformation("JwtBearer.OnTokenValidated: Token validated. Claims: {claims}", claims);
                logger.LogInformation("JwtBearer.OnTokenValidated: IsInRole('Admin') => {isAdmin}", ctx.Principal!.IsInRole("Admin"));
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(ctx.Exception, "JwtBearer.OnAuthenticationFailed: Authentication failed");
                if (ctx.Exception != null)
                {
                    // also write the exception message for quick inspection
                    logger.LogWarning("JwtBearer.OnAuthenticationFailed: Exception message: {msg}", ctx.Exception.Message);
                }
                return Task.CompletedTask;
            }
        };

    });

// Authorization (default)
builder.Services.AddAuthorization();

var app = builder.Build();

// --- HTTP pipeline --------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS System API v1");
        c.RoutePrefix = "swagger"; // UI at /swagger
    });

    // convenience: redirect root to swagger in dev
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

// quick ping
app.MapGet("/ping", () => Results.Ok("pong")).AllowAnonymous();

// debug routes listing
app.MapGet("/debug/routes", (EndpointDataSource eds) =>
{
    var routes = eds.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => $"{e.RoutePattern.RawText} -> {e.DisplayName}");
    return Results.Text(string.Join(Environment.NewLine, routes));
}).RequireAuthorization(); // protect debug route if you wish (optional)

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

// Diagnostic: log request summary & Authorization header preview (optional)
// Place before auth so we can see header presence
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var authHeader = context.Request.Headers.TryGetValue("Authorization", out var val) ? val.ToString() : null;
    var preview = authHeader == null ? "<none>" : (authHeader.Length <= 64 ? authHeader : authHeader.Substring(0, 64) + "...");
    logger.LogInformation("REQ {Method} {Path} | Authorization header present: {HasAuth} | Preview: {Preview}",
        context.Request.Method, context.Request.Path, authHeader != null, preview);
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await SeedRolesAndAdminAsync(app);

app.Run();

// --- Helper: seed roles & admin user -------------------------------------
static async Task SeedRolesAndAdminAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var roleDefinitions = new[]
    {
        new { Name = "Admin", Description = "System Administrator with full access" },
        new { Name = "Manager", Description = "Branch Manager with management privileges" },
        new { Name = "Cashier", Description = "Cashier with sales transaction access" },
        new { Name = "StockClerk", Description = "Stock management and inventory access" },
        new { Name = "Accountant", Description = "Financial reporting and accounting access" }
    };

    foreach (var r in roleDefinitions)
    {
        if (!await roleManager.RoleExistsAsync(r.Name))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = r.Name, Description = r.Description });
        }
    }

    var adminEmail = "admin@pos.local";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FullName = "System Admin",
            BranchId = "BRANCH_MAIN",
            BranchName = "Main Branch",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@1234!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            foreach (var err in result.Errors)
                Console.WriteLine($"Error creating admin user: {err.Description}");
        }
    }
}

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
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Services --------------------------------------------------------------
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

// DB contexts (keep your connection strings in appsettings / user-secrets)

builder.Services.AddDbContext<PosSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PosSystemConnectionString")));
builder.Services.AddDbContext<PosSystemAuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PosSystemAuthConnectionString")));


// App services / repos
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductLineItemRepository, ProductLineItemRepository>();
builder.Services.AddScoped<IProductLineItemService, ProductLineItemService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderRepository,  OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderServise>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfiles>());

// Jwt settings (user-secrets in Development)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not found.");
// Temporary debug: Log JWT settings to console
//Console.WriteLine($"JWT Key (length {jwtSettings.Key.Length}): {jwtSettings.Key}");
//Console.WriteLine($"JWT Issuer: {jwtSettings.Issuer}");
//Console.WriteLine($"JWT Audience: {jwtSettings.Audience}");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", p => p
        .WithOrigins(
            "http://localhost:4200",
            "https://pos-frontend-murex.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ------------------ AUTHENTICATION (JWT only) ------------------------------

// Preserve claim names exactly as you emit them
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

// Configure JWT Bearer
builder.Services.AddAuthentication(options =>
{
    // Set the default authentication scheme to JWT Bearer
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        // For local/dev convenience this example uses relaxed issuer/audience checks.
        // For production set ValidateIssuer/Audience = true and supply correct values.
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
            //NameClaimType = "nameid",
            //RoleClaimType = "role"

            // Specify which claim to use for the user's name
            NameClaimType = ClaimTypes.Name,
            // Specify which claim to use for roles
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
//builder.Services.AddAuthorization();

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // keep the same password/lockout/user settings you had
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

// Prevent cookie auto-redirect for APIs
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
});

// --- Build app -------------------------------------------------------------
var app = builder.Build();

// Always use swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS System API v1");
        c.RoutePrefix = "swagger"; // swagger at site root in dev
    });


app.UseHttpsRedirection();
// 1. First, enable routing.
app.UseRouting();
app.UseCors("AllowAngularApp");

// Diagnostic: log request summary (before auth)
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

// debug route (unprotected so you can always call it)
app.MapGet("/debug/routes", (EndpointDataSource eds) =>
{
    var routes = eds.Endpoints.OfType<RouteEndpoint>().Select(e => $"{e.RoutePattern.RawText} -> {e.DisplayName}");
    return Results.Text(string.Join(Environment.NewLine, routes));
}).AllowAnonymous();

// Seed roles / admin (your existing method)
await SeedRolesAndAdminAsync(app);

app.Run();

// ----------------- Seed roles helper -------------------
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
            await roleManager.CreateAsync(new ApplicationRole { Name = r.Name, Description = r.Description });
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
            await userManager.AddToRoleAsync(adminUser, "Admin");
        else
            foreach (var err in result.Errors)
                Console.WriteLine($"Error creating admin user: {err.Description}");
    }
}

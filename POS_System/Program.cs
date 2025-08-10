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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Database contexts
builder.Services.AddDbContext<PosSystemDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("PosSystemConnectionString")));

builder.Services.AddDbContext<PosSystemAuthDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("PosSystemAuthConnectionString")));

// Register repositories and services

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IProductLineItemRepository, ProductLineItemRepository>();
builder.Services.AddScoped<IProductLineItemService, ProductLineItemService>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

//AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// Configure strongly typed JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not found.");


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero // optional: prevent time drift
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

async Task SeedRolesAndAdminAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var roles = new[] { "Admin", "Manager", "Cashier", "StockClerk", "Accountant" };
    foreach (var r in roles)
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new IdentityRole(r));

    var adminEmail = "admin@pos.local";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FullName = "System Admin",
            BranchId = "BRANCH_MAIN",   // Seed default branch code
            BranchName = "Main Branch"
        };
        await userManager.CreateAsync(adminUser, "Admin@1234!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
await SeedRolesAndAdminAsync(app);
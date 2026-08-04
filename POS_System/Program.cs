using POS_System.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Keep user secrets in development for JWT / sensitive settings
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Consolidated service registrations using extension methods for clarity
builder.Services.AddPosSystemServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure middleware / pipeline
app.UsePosSystem();

// Apply migrations and seed roles/admin
await app.MigrateAndSeedAsync();

app.Run();

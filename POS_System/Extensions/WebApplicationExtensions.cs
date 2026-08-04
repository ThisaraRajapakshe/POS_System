using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace POS_System.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UsePosSystem(this WebApplication app)
        {
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger(c => c.RouteTemplate = "api/swagger/{documentName}/swagger.json");
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "POS System API v1");
                    c.RoutePrefix = "api/swagger";
                });
            }

            app.UseHttpsRedirection();
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

            return app;
        }

        public static async Task MigrateAndSeedAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<PosSystemDbContext>();
                    await context.Database.MigrateAsync();

                    var authContext = services.GetRequiredService<PosSystemAuthDbContext>();
                    await authContext.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            await SeedRolesAndAdminAsync(app);
        }

        private static async Task SeedRolesAndAdminAsync(IHost host)
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
    }
}

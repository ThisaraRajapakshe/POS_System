using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Mapping;
using POS_System.Repositories;
using POS_System.Repositories.Implementation;
using POS_System.ApplicationServices;
using POS_System.ApplicationServices.Implementation;

namespace POS_System.Extensions
{
    public static class PersistenceAndServicesExtensions
    {
        public static IServiceCollection AddPersistenceAndApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContexts
            services.AddDbContext<PosSystemDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("PosSystemConnectionString")));

            services.AddDbContext<PosSystemAuthDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("PosSystemAuthConnectionString")));

            // Repositories & services
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductLineItemRepository, ProductLineItemRepository>();
            services.AddScoped<IProductLineItemService, ProductLineItemService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderServise>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IReportService, ReportService>();
            services.AddSingleton<TimeZoneHelper>();
            services.AddScoped<IDashboardsService, DashboardsService>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfiles>());

            // CORS (kept with persistence/services for now)
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", p => p
                    .WithOrigins(
                        "http://localhost:4200",
                        "http://13.206.165.125",
                        "https://thisara.dev",
                        "https://www.thisara.dev")
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            return services;
        }
    }
}

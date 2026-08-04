namespace POS_System.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPosSystemServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            // Presentation basics
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            // Delegate detailed registrations to focused extension methods
            services.AddPosSystemSwagger();
            services.AddPersistenceAndApplicationServices(configuration);
            services.AddPosSystemIdentity(configuration);
            services.AddPosSystemJwtAuthentication(configuration);

            return services;
        }
    }
}

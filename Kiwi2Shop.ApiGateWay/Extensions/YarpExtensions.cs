namespace Kiwi2Shop.GateWay.Extensions
{
    public static class YarpExtensions
    {
        public static IServiceCollection AddYarpReverseProxy(this IServiceCollection services, IConfiguration configuration)
        {
            // Add service discovery for resolving service names
            services.AddServiceDiscovery();

            // Configure YARP with service discovery
            services.AddReverseProxy()
                .LoadFromConfig(configuration.GetSection("ReverseProxy"))
                .AddServiceDiscoveryDestinationResolver();

            return services;
        }

        public static IServiceCollection AddGatewayCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                // Allow frontend access with credentials
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173", "https://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });

                // Allow internal service-to-service communication
                options.AddPolicy("InternalServices", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}


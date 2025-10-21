using Application.Interfaces;
using Application.Interfaces.Repositories;
using Azure.Messaging.ServiceBus;
using Domain.Primitives;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using StackExchange.Redis;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPersistenceSQLServer(configuration);
            services.AddAzureServiceBus(configuration);
            services.AddRedis(configuration);
            services.AddMinio(configuration);
            services.AddSupabase(configuration);

            return services;
        }

        private static IServiceCollection AddPersistenceSQLServer(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("DatabaseConnection") ?? throw new InvalidOperationException("Missing 'DatabaseConnection'.");

            // Si tu API crea muchos DbContext por request, el pooling reduce GC/allocs.
            services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlServer(conn, sql =>
                {
                    // Resiliencia ante errores transitorios (Azure SQL / redes)
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    // command timeout razonable para cargas pesadas
                    sql.CommandTimeout(60);
                });
            });

            // Exponer la interfaz de contexto y UoW desde el mismo DbContext
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Repositorios
            services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));
            services.AddScoped<ICompanyRepository, CompanyRepository>();

            return services;
        }

        // Codigo que permite agregar una Base de datos PostgreSQL
        //private static IServiceCollection AddPersistencePostgreSQL(this IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
        //    {
        //        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DatabaseConnection"),
        //            options => options.EnableRetryOnFailure());
        //    });

        //    services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        //    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        //    // Repositories dependency injection
        //    services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));
        //    services.AddScoped<ICompanyRepository, CompanyRepository>();

        //    return services;
        //}

        private static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ServiceBusClient>(options =>
            {
                var connectionString = configuration.GetConnectionString("AzureServiceBus");
                return new ServiceBusClient(connectionString);
            });

            services.AddScoped<IMessageBusService, AzureServiceBusService>();


            return services;
        }

        private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(options =>
            {
                var redisSettings = configuration.GetSection("RedisSettings").Get<RedisSettings>()
                    ?? throw new InvalidOperationException("RedisSettings configuration section is missing or invalid.");
                return ConnectionMultiplexer.Connect(redisSettings!.ConnectionString);
            });

            services.AddTransient<IRedisCacheService, RedisCacheService>();

            return services;
        }

        public static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration)
        {
            var minioSettings = configuration.GetSection("MinioSettings").Get<MinioSettings>() 
                ?? throw new InvalidOperationException("MinioSettings configuration section is missing or invalid.");

            services.AddSingleton(minioSettings!);

            services.AddSingleton<IMinioClient>(sp =>
            {
                return new MinioClient()
                    .WithEndpoint(minioSettings!.Endpoint)
                    .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey)
                    .Build();
            });

            // (opcional) Podrías registrar también un servicio que use MinioClient aquí
            services.AddTransient<IFileStorageService, MinioFileStorageService>();

            // (opcional) Podrías registrar un servicio para inicializar el bucket de Minio
            services.AddHostedService<MinioBucketInitializerService>();

            return services;
        }

        private static IServiceCollection AddSupabase(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("Supabase");
            var supabaseSettings = section.Get<SupabaseSettings>()
                ?? throw new InvalidOperationException("Supabase configuration section is missing or invalid.");

            // Registra como options + value
            services.Configure<SupabaseSettings>(section);
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<SupabaseSettings>>().Value);

            // Derivar URL si falta
            var baseUrl = !string.IsNullOrWhiteSpace(supabaseSettings.Url)
                ? supabaseSettings.Url
                : (!string.IsNullOrWhiteSpace(supabaseSettings.ProjectRef)
                    ? $"https://{supabaseSettings.ProjectRef}.supabase.co"
                    : throw new InvalidOperationException("Supabase Url or ProjectRef must be provided."));

            services.AddHttpClient("Supabase", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                if (!string.IsNullOrEmpty(supabaseSettings.ServiceRoleKey))
                {
                    client.DefaultRequestHeaders.Add("apikey", supabaseSettings.ServiceRoleKey);
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseSettings.ServiceRoleKey}");
                }
            });

            services.AddTransient<ISupabaseService, SupabaseService>();
            return services;
        }
    }
}

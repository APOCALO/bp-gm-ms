using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using Framework.Application.Interfaces.Repositories;
using Framework.Domain.Primitives;
using Framework.Infrastructure.Data;
using Framework.Infrastructure.Persistence.Repositories;
using Framework.Web.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;
using Web.Api.Application.Interfaces.Repositories;
using Web.Api.Infrastructure.Data;
using Web.Api.Infrastructure.Repositories;

namespace Web.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatRConfig();
            services.AddFluentValidationConfig();
            services.AddAutoMapperConfig();

            // Esto es del framework, no de la API
            services.AddWebApiServices(configuration);

            // HealthChecks concretos (DB, Redis, ServiceBus)
            var hc = services.AddHealthChecks();  // obtiene IHealthChecksBuilder

            // HealthCheck Database
            hc.AddDbContextCheck<ApplicationDbContext>(name: "Database");

            // Added database
            services.AddPersistenceSQLServer(configuration);

            services.AddControllers()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    // opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            // 1) API Versioning (URL segment + header + query) + ApiExplorer para Swagger
            services.AddApiVersioningConfig();

            // 2) Swagger (con seguridad Bearer y docs por versión)
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // XML comments
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);

                // Seguridad Bearer
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Autenticación JWT con Bearer.\r\n\r\nEjemplo: **Bearer ey...**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };
                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
            });

            // Registra configurador que crea un SwaggerDoc por cada versión detectada
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            // CORS
            AddCors(services);

            return services;
        }

        private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyReference>();

            return services;
        }

        private static IServiceCollection AddAutoMapperConfig(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }

        private static IServiceCollection AddMediatRConfig(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblyContaining<ApplicationAssemblyReference>();
            });

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

        private static IServiceCollection AddCors(this IServiceCollection services)
        {
            services.AddCors(o => o.AddDefaultPolicy(p =>
            {
                p.WithOrigins("http://localhost:3000", "https://tu-frontend.com")
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials();
            }));

            return services;
        }

        private static IServiceCollection AddApiVersioningConfig(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true; // agrega headers api-supported/deprecated
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),              // /api/v{version}/...
                    new HeaderApiVersionReader("api-version"),     // header: api-version: 1.0
                    new QueryStringApiVersionReader("api-version") // ?api-version=1.0
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";          // v1, v1.1, v2
                options.SubstituteApiVersionInUrl = true;    // reemplaza {version} en la ruta
            });

            return services;
        }

        // Configurador para generar un SwaggerDoc por cada versión descubierta por IApiVersionDescriptionProvider
        private sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
        {
            private readonly IApiVersionDescriptionProvider _provider;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

            public void Configure(SwaggerGenOptions options)
            {
                foreach (var desc in _provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                    {
                        Title = "Web.Api",
                        Version = desc.ApiVersion.ToString(),
                        Description = desc.IsDeprecated ? "DEPRECATED" : "HTTP API"
                    });
                }
            }
        }
    }
}

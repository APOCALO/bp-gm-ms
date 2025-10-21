using Asp.Versioning.ApiExplorer;
using Framework.Application;
using Framework.Infrastructure;
using Framework.Infrastructure.Auth;
using Framework.Web.Api;
using Serilog;
using Web.Api;
using Web.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1) Configurar Serilog ANTES de construir el contenedor
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // lee appsettings.json
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(); // hace que ILogger<> use Serilog

// Hacer que solo Serilog maneje logs:
builder.Logging.ClearProviders();

try
{
    Log.Information("Bootstrapping application...");

    // 2) Registrar servicios
    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddPresentation(builder.Configuration);

    // Supabase auth
    builder.Services.AddSupabaseAuthentication(builder.Configuration);

    var app = builder.Build();

    // 3) Request logging de Serilog (útil para trazabilidad)
    app.UseSerilogRequestLogging(options =>
    {
        // puedes enriquecer el DiagnosticContext si lo necesitas
        // options.EnrichDiagnosticContext = (ctx, http) => { ... };
    });

    Log.Information("Application setup completed successfully.");

    // 4)  Configure
    app.UseWebApiServices();


    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseSwaggerUI(options =>
        {
            foreach (var desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
            }
        });

        // Apply the database migrations
        app.ApplyMigrations();
    }

    app.UseHttpsRedirection();

    // CORS
    app.UseCors();

    // AuthN -> AuthZ
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Starting the application...");
    app.Run();
}
catch (Exception ex)
{
    // Log crítico si el host no puede iniciar o cae fatalmente
    Log.Fatal(ex, "Host terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    // Asegura que todos los logs se vacíen
    Log.CloseAndFlush();
}

// Necesario para pruebas integradas
public partial class Program { }

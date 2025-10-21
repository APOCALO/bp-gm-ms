using Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;

namespace Infrastructure.Auth
{
    public static class SupabaseAuthExtensions
    {
        public static IServiceCollection AddSupabaseAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var supabase = configuration.GetSection("Supabase").Get<SupabaseSettings>()
                ?? throw new InvalidOperationException("Supabase configuration section is missing or invalid.");

            var baseUrl = supabase.ResolvedUrl.TrimEnd('/');
            var issuer = $"{baseUrl}/auth/v1";
            var jwksUri = $"{issuer}/.well-known/jwks.json";

            services.AddHttpClient("SupabaseAuth");
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Preferimos HS256 con JWT secret si está configurado
            if (!string.IsNullOrWhiteSpace(supabase.JwtSecret))
            {
                var keyBytes = Encoding.UTF8.GetBytes(supabase.JwtSecret);
                var symmetricKey = new SymmetricSecurityKey(keyBytes);

                services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidIssuer = issuer,
                            ValidateIssuer = true,

                            ValidAudience = "authenticated",
                            ValidateAudience = true,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = symmetricKey,   // HS256 🔐

                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.FromMinutes(2),
                            NameClaimType = JwtRegisteredClaimNames.Sub
                        };
                    });

                services.AddAuthorization();
                return services;
            }

            // Fallback: intentar JWKS (para setups con RS256)
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = issuer,
                        ValidateIssuer = true,

                        ValidAudience = "authenticated",
                        ValidateAudience = true,

                        ValidateIssuerSigningKey = true,

                        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                        {
                            var sp = services.BuildServiceProvider();
                            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("SupabaseAuth");
                            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("SupabaseAuth");

                            try
                            {
                                var jwks = http.GetFromJsonAsync<JsonWebKeySet>(jwksUri).GetAwaiter().GetResult();
                                if (jwks is null || jwks.Keys.Count == 0)
                                {
                                    logger?.LogWarning("JWKS vacío desde {JwksUri}. Tu proyecto probablemente usa HS256. Configura Supabase:JwtSecret.", jwksUri);
                                    return Array.Empty<SecurityKey>();
                                }

                                return jwks.Keys.Select(k => (SecurityKey)k).ToArray();
                            }
                            catch (Exception ex)
                            {
                                logger?.LogError(ex, "Error obteniendo JWKS desde {JwksUri}", jwksUri);
                                return Array.Empty<SecurityKey>();
                            }
                        },

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2),
                        NameClaimType = JwtRegisteredClaimNames.Sub
                    };
                });

            services.AddAuthorization();
            return services;
        }
    }
}

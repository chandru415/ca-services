using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Presentation.Installers.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace Presentation.Installers.InstallServices
{
    public class KeycloakInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Keycloak:Authority"];
                options.Audience = configuration["Keycloak:Audience"];
                options.MetadataAddress = configuration["Keycloak:MetadataAddress"] ?? "";
                options.RequireHttpsMetadata = false; // Disable in development
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Keycloak:ValidIssuer"],
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero, // Strict time validation
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        if (context.SecurityToken is JwtSecurityToken accessToken)
                        {
                            context.HttpContext.Items["access_token"] = accessToken.RawData;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddHttpContextAccessor();
        }
    }
}

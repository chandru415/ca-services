using Microsoft.AspNetCore.OpenApi;
using Presentation.Installers.Interfaces;

namespace Presentation.Installers.InstallServices
{
    public class OpenApiInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            services.AddEndpointsApiExplorer();
            services.AddOpenApi();
        }
    }
}

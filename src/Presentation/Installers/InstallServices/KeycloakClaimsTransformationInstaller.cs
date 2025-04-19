using Microsoft.AspNetCore.Authentication;
using Presentation.Installers.Interfaces;
using Presentation.Installers.Transformations;

namespace Presentation.Installers.InstallServices
{
    public class KeycloakClaimsTransformationInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();
        }
    }
}

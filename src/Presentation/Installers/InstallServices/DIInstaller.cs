using Application;
using Presentation.Installers.Interfaces;

namespace Presentation.Installers.InstallServices
{
    public class DIInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthorization();
            services.AddApplication();
        }
    }
}

using Presentation.Installers.Interfaces;

namespace Presentation.Installers.InstallServices
{
    public class CorsInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "MyAllowSpecificOrigins",
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin() 
                                             .AllowAnyHeader()
                                             .AllowAnyMethod();
                                  });
            });
        }
    }
}

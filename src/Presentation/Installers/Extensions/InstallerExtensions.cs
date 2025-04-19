using OpenTelemetry.Logs;
using Presentation.Installers.Interfaces;

namespace Presentation.Installers.Extensions
{
    public static class InstallerExtensions
    {
        public static void InstallServicesInAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            typeof(Program).Assembly.ExportedTypes
                .Where(x => typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance).Cast<IInstaller>()
                .ToList()
                .ForEach(installer => installer.InstallServices(services, configuration));
        }


        public static void BuilderExtensionsInAssembly(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(configuration["OTLP_ENDPOINT_URL"] ?? "http://localhost:4317");
                otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            }));
        }
    }
}

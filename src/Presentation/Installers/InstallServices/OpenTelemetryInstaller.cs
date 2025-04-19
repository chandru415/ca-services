using Presentation.Installers.Interfaces;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Presentation.Installers.Extensions;

namespace Presentation.Installers.InstallServices
{
    public class OpenTelemetryInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Get configuration values with proper fallbacks
            var tracingOtlpEndpoint = configuration["OTLP_ENDPOINT_URL"] ?? "http://localhost:4317";
            var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            var serviceName = configuration["SERVICE_NAME"] ?? "UnknownService";

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
                        serviceInstanceId: Environment.MachineName)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["environment"] = environmentName,
                        ["deployment.region"] = configuration["REGION"] ?? "local"
                    }))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.Filter = ctx => ctx.Request.Path != "/health"; // Exclude health checks
                            options.EnrichWithHttpRequest = (activity, request) =>
                            {
                                activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress);
                            };
                        })
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddSource("MediatR")
                        ;

                    if (Uri.TryCreate(tracingOtlpEndpoint, UriKind.Absolute, out var otlpUri))
                    {
                        tracing.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = otlpUri;
                            otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        });
                    }
                    else
                    {
                        tracing.AddConsoleExporter();
                        services.AddSingleton<ILogger>(provider =>
                            provider.GetRequiredService<ILogger<Program>>());
                    }
                })
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("NATS.*")
                    .AddMeter("MediatR.*")
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("System.Net.Http")
                    .AddMeter("System.Net.NameResolution")
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                        otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    }));
        }
    }
}

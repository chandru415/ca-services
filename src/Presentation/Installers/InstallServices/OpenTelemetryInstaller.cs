using Presentation.Installers.Interfaces;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Presentation.Installers.Extensions;
using OpenTelemetry.Exporter;

namespace Presentation.Installers.InstallServices
{
    public class OpenTelemetryInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var otlpEndpoint = configuration["OTLP_ENDPOINT_URL"] ?? "http://localhost:4317";

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.ParseStateValues = true;
                    options.IncludeScopes = true;
                });
            });

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(
                        serviceName: configuration["SERVICE_NAME"] ?? "UnknownService",
                        serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
                        serviceInstanceId: Environment.MachineName)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                        ["deployment.region"] = configuration["REGION"] ?? "local"
                    }))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.Filter = ctx => ctx.Request.Path != "/health";
                            options.EnrichWithHttpRequest = (activity, request) =>
                            {
                                activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress);
                            };
                        })
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddSource("MediatR")
                        .AddSource("NATS.Client")
                        .AddSource("MyApp.MediatR")
                        .AddSource("Redis") // If you're using a Redis library with diagnostics
                        .SetErrorStatusOnException();

                    if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var otlpUri))
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
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddMeter("NATS.*")
                        .AddMeter("MediatR.*")
                        .AddMeter("Redis")
                        .AddMeter("Microsoft.AspNetCore.Hosting")
                        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                        .AddMeter("System.Net.Http")
                        .AddMeter("System.Net.NameResolution")
                        .AddMeter("MyApp.Metrics");

                    if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var metricEndpoint))
                    {
                        metrics.AddOtlpExporter(options =>
                        {
                            options.Endpoint = metricEndpoint;
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                    }
                })
                .WithLogging(logging =>
                {
                    logging.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
                });
        }

    }
}

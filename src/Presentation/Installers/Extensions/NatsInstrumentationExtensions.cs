using OpenTelemetry.Trace;

namespace Presentation.Installers.Extensions
{
    public static class NatsInstrumentationExtensions
    {
        public static TracerProviderBuilder AddNatsInstrumentation(this TracerProviderBuilder builder)
        {
            return builder.AddSource("NATS.Client", "NATS.Net", "NATS.Producer");
        }

    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Behaviours
{
    public class OpenTelemetryPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private static readonly ActivitySource ActivitySource = new("MyApp.MediatR");
        private static readonly Meter Meter = new("MyApp.MediatR.Metrics");

        private static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>(
            "mediatr.requests.count",
            description: "Number of MediatR requests processed");

        private static readonly Histogram<double> DurationHistogram = Meter.CreateHistogram<double>(
            "mediatr.request.duration",
            unit: "ms",
            description: "Duration of MediatR request processing");

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var responseName = typeof(TResponse).Name;
            var startTime = Stopwatch.GetTimestamp();

            var tags = new TagList
        {
            { "request.type", requestName },
            { "response.type", responseName },
            { "request.source", Activity.Current?.Source?.Name ?? "unknown" }
        };

            using var activity = ActivitySource.StartActivity($"MediatR:{requestName}", ActivityKind.Internal);

            if (activity != null)
            {
                activity.SetTag("request.type", typeof(TRequest).FullName);
                activity.SetTag("request.timestamp", DateTime.UtcNow);
            }

            RequestCounter.Add(1, tags);

            try
            {
                var response = await next(cancellationToken);

                var durationMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
                DurationHistogram.Record(durationMs, tags);

                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetTag("duration.ms", durationMs);

                return response;
            }
            catch (Exception ex)
            {
                var durationMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
                DurationHistogram.Record(durationMs, tags);

                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error.type", ex.GetType().FullName);
                activity?.AddException(ex);
                activity?.SetTag("duration.ms", durationMs);

                throw;
            }
        }
    }
}

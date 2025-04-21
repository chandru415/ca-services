using Application.Common.ContextServices;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Helpers;
using System.Diagnostics;

namespace Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest, TResponse>(ILogger<LoggingBehaviour<TRequest, TResponse>> logger, UserContextService userContextService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {

        private readonly UserContextService _userContextService = userContextService;
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger = logger;
        private static readonly ActivitySource activitySource = new("MyApp.MediatR");

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var traceId = Activity.Current?.TraceId.ToString() ?? "none";
            var spanId = Activity.Current?.SpanId.ToString() ?? "none";
            var username = _userContextService.GetCurrentUsername() ?? "anonymous";

            using var activity = activitySource.StartActivity($"MediatR:{requestName}", ActivityKind.Internal);
            activity?.SetTag("request.name", requestName);
            activity?.SetTag("trace.id", traceId);
            activity?.SetTag("span.id", spanId);
            activity?.SetTag("username", username);
            activity?.SetTag("request.payload", Helper.SafeSerialize(request));

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["Request"] = requestName,
                ["TraceId"] = traceId,
                ["SpanId"] = spanId
            }))
            {
                _logger.LogInformation("➡️  Handling {Request}", requestName);

                var sw = Stopwatch.StartNew();
                try
                {
                    var response = await next(cancellationToken);
                    sw.Stop();

                    _logger.LogInformation("✅ Handled {Request} in {Elapsed}ms for user {username}", requestName, sw.ElapsedMilliseconds, username);

                    activity?.SetTag("response.payload", Helper.SafeSerializeResponse(response));

                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return response;
                }
                catch (Exception ex)
                {
                    sw.Stop();

                    _logger.LogError(ex, "❌ Failed {Request} after {Elapsed}ms - {Error} for user {username}", requestName, sw.ElapsedMilliseconds, ex.Message, username);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);

                    throw;
                }
            }
        }
    }


}

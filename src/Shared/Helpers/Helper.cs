using System.Text.Encodings.Web;
using System.Text.Json;

namespace Shared.Helpers
{
    public static class Helper
    {
        public static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public static object SafeSerialize<TRequest>(TRequest request) where TRequest : notnull
        {
            try
            {
                return JsonSerializer.Serialize(request, options: GetOptions());
            }
            catch
            {
                return "[Serialization Error]";
            }
        }

        public static object SafeSerializeResponse<TResponse>(TResponse response)
        {
            try
            {
                return JsonSerializer.Serialize(response, options: GetOptions());
            }
            catch
            {
                return "[Serialization Error]";
            }
        }
    }
}

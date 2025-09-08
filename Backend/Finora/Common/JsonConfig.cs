using System.Text.Json;

namespace Finora.Backend.Common
{
    internal static class JsonConfig
    {
        public static JsonSerializerOptions JsonOptions => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
}

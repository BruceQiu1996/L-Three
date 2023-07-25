using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ThreeL.Infra.Core.Serializer
{
    public static class SystemTextJson
    {
        public static JsonSerializerOptions GetAdncDefaultOptions(Action<JsonSerializerOptions>? configOptions = null)
        {
            var options = new JsonSerializerOptions();
            options.Encoder = GetAdncDefaultEncoder();
            options.ReadCommentHandling = JsonCommentHandling.Skip;
            options.PropertyNameCaseInsensitive = true;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            configOptions?.Invoke(options);
            return options;
        }

        public static JavaScriptEncoder GetAdncDefaultEncoder() => JavaScriptEncoder.Create(new TextEncoderSettings(UnicodeRanges.All));
    }
}

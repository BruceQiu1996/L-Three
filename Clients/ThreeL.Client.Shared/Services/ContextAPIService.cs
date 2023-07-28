using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using ThreeL.Client.Shared.Configurations;

namespace ThreeL.Client.Shared.Services
{
    public class ContextAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ContextAPIOptions _contextAPIOptions;

        public ContextAPIService(IOptions<ContextAPIOptions> contextAPIOptions)
        {
            _contextAPIOptions = contextAPIOptions.Value;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"http://{_contextAPIOptions.Host}:{_contextAPIOptions.Port}/api/");
            _httpClient.Timeout = TimeSpan.FromSeconds(600); //Test
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetToken(string token)
        {
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            }
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<HttpResponseMessage> PostAsync(string url, dynamic body)
        {
            var content = new StringContent(JsonSerializer.Serialize(body));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resp = await _httpClient.PostAsync(url, content);

            return resp;
        }

        public async Task<HttpResponseMessage> PutAsync(string url, dynamic body)
        {
            var content = new StringContent(JsonSerializer.Serialize(body));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resp = await _httpClient.PutAsync(url, content);

            return resp;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _httpClient.GetAsync(url);
        }
    }
}

using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Infra.Core.Serializer;

namespace ThreeL.Client.Shared.Services
{
    public class ContextAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ContextAPIOptions _contextAPIOptions;
        private readonly JsonSerializerOptions _jsonOptions = SystemTextJson.GetAdncDefaultOptions();

        public ContextAPIService(IOptions<ContextAPIOptions> contextAPIOptions)
        {
            _contextAPIOptions = contextAPIOptions.Value;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"http://{_contextAPIOptions.Host}:{_contextAPIOptions.Port}/api/");
            _httpClient.Timeout = TimeSpan.FromSeconds(600); //Test
            _httpClient.DefaultRequestVersion = HttpVersion.Version10;
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

        public async Task<T> PostAsync<T>(string url, dynamic body, bool excuted = false)
        {
            var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resp = await _httpClient.PostAsync(url, content);
            if (resp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), _jsonOptions);
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized && !excuted)
            {
                var result = await TryRefreshTokenAsync?.Invoke();
                if (!result)
                {
                    await ExcuteWhileUnauthorizedAsync?.Invoke();
                    return default;
                }
                excuted = true;
                return await GetAsync<T>(url, excuted);
            }
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await resp.Content.ReadAsStringAsync();
                await ExcuteWhileBadRequestAsync?.Invoke(message);
            }

            return default;
        }

        public async Task<T> GetAsync<T>(string url, bool excuted = false)
        {
            var resp = await _httpClient.GetAsync(url);
            if (resp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), _jsonOptions);
            }
            else if (resp.StatusCode == HttpStatusCode.Unauthorized && !excuted)
            {
                var result = await TryRefreshTokenAsync?.Invoke();
                if (!result)
                {
                    await ExcuteWhileUnauthorizedAsync?.Invoke();
                    return default;
                }
                excuted = true;
                return await GetAsync<T>(url, excuted);
            }
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await resp.Content.ReadAsStringAsync();
                await ExcuteWhileBadRequestAsync?.Invoke(message);
            }

            return default;
        }

        public async Task<T> RefreshTokenAsync<T>(dynamic body) 
        {
            var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resp = await _httpClient.PostAsync("refresh/token", content);
            if (resp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), _jsonOptions);
            }

            return default;
        }

        public Func<Task<bool>> TryRefreshTokenAsync { get; set; } //当服务端返回401的时候，尝试利用refreshtoken重新获取accesstoken以及refreshtoken
        public Func<Task> ExcuteWhileUnauthorizedAsync { get; set; } //401
        public Func<string, Task> ExcuteWhileBadRequestAsync { get; set; } //400
    }
}

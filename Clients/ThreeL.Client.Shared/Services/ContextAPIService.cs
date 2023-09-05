using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using System.Text.Json;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Infra.Core.Serializer;

namespace ThreeL.Client.Shared.Services
{
    public class ContextAPIService
    {
        private string _token;
        private readonly HttpClient _httpClient;
        private readonly ContextAPIOptions _contextAPIOptions;
        private readonly JsonSerializerOptions _jsonOptions = SystemTextJson.GetAdncDefaultOptions();

        public ContextAPIService(IOptions<ContextAPIOptions> contextAPIOptions)
        {
            _contextAPIOptions = contextAPIOptions.Value;
            _httpClient = new HttpClient();
            BuildHttpClient(_httpClient);
        }

        public void SetToken(string token)
        {
            _token = token;
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            }
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<T> PostAsync<T>(string url, dynamic body, bool excuted = false)
        {
            HttpResponseMessage resp = null;
            if (body == null)
            {
                resp = await _httpClient.PostAsync(url, null);
            }
            else
            {
                var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                resp = await _httpClient.PostAsync(url, content);
            }
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
            var resp = await _httpClient.GetAsync(url).ConfigureAwait(false);
            if (resp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false), _jsonOptions);
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
                var message = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                await ExcuteWhileBadRequestAsync?.Invoke(message);
            }

            return default;
        }

        public async Task<T> UploadFileAsync<T>(string filename, byte[] bytes, string code, bool isGroup, long receiver,
                                                Action<object, HttpProgressEventArgs> progressCallBack = null, bool excuted = false)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler(httpClientHandler);
            if (progressCallBack != null)
            {
                progressMessageHandler.HttpSendProgress += (obj, e) => progressCallBack(obj, e);
            }
            using (var client = new HttpClient(progressMessageHandler))
            {
                BuildHttpClient(client);
                if (!string.IsNullOrEmpty(_token))
                {
                    if (client.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        client.DefaultRequestHeaders.Remove("Authorization");
                    }
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
                }
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = filename
                };
                var content = new MultipartFormDataContent
                {
                    fileContent
                };

                var resp = await client.PostAsync(string.Format(Const.UPLOAD_FILE, isGroup, receiver, code), content);
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
                    return await UploadFileAsync<T>(filename, bytes, code, isGroup, receiver, progressCallBack, excuted);
                }
                else if (resp.StatusCode == HttpStatusCode.BadRequest)
                {
                    var message = await resp.Content.ReadAsStringAsync();
                    await ExcuteWhileBadRequestAsync?.Invoke(message);
                }
            }

            return default;
        }

        public async Task<byte[]> UploadUserAvatarAsync(string filename, byte[] bytes, string code,
                                                      Action<object, HttpProgressEventArgs> progressCallBack = null, bool excuted = false)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler(httpClientHandler);
            if (progressCallBack != null)
            {
                progressMessageHandler.HttpSendProgress += (obj, e) => progressCallBack(obj, e);
            }
            using (var client = new HttpClient(progressMessageHandler))
            {
                BuildHttpClient(client);
                if (!string.IsNullOrEmpty(_token))
                {
                    if (client.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        client.DefaultRequestHeaders.Remove("Authorization");
                    }
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
                }
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = filename
                };
                var content = new MultipartFormDataContent
                {
                    fileContent
                };

                var resp = await client.PostAsync(string.Format(Const.UPLOAD_AVATAR, code), content);
                if (resp.IsSuccessStatusCode)
                {
                    var steam = await resp.Content.ReadAsByteArrayAsync();
                    return steam;
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
                    return await UploadUserAvatarAsync(filename, bytes, code, progressCallBack, excuted);
                }
                else if (resp.StatusCode == HttpStatusCode.BadRequest)
                {
                    var message = await resp.Content.ReadAsStringAsync();
                    await ExcuteWhileBadRequestAsync?.Invoke(message);
                }
            }

            return default;
        }

        public async Task<byte[]> DownloadUserAvatarAsync(long userId, long avatarId, Action<object, HttpProgressEventArgs> progressCallBack = null, bool excuted = false)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler(httpClientHandler);
            if (progressCallBack != null)
            {
                progressMessageHandler.HttpReceiveProgress += (obj, e) => progressCallBack(obj, e);
            }
            using (var client = new HttpClient(progressMessageHandler))
            {
                BuildHttpClient(client);
                if (!string.IsNullOrEmpty(_token))
                {
                    if (client.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        client.DefaultRequestHeaders.Remove("Authorization");
                    }
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
                }

                var resp = await client.GetAsync(string.Format(Const.DOWNLOAD_AVATAR, userId, avatarId));
                if (resp.IsSuccessStatusCode)
                {
                    var steam = await resp.Content.ReadAsByteArrayAsync();
                    return steam;
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
                    return await DownloadUserAvatarAsync(userId, avatarId, progressCallBack, excuted);
                }
                else if (resp.StatusCode == HttpStatusCode.BadRequest)
                {
                    var message = await resp.Content.ReadAsStringAsync();
                    await ExcuteWhileBadRequestAsync?.Invoke(message);
                }
            }

            return default;
        }

        public async Task<byte[]> DownloadFileAsync(string messageId, Action<object, HttpProgressEventArgs> progressCallBack = null, bool excuted = false)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler(httpClientHandler);
            if (progressCallBack != null)
            {
                progressMessageHandler.HttpReceiveProgress += (obj, e) => progressCallBack(obj, e);
            }
            using (var client = new HttpClient(progressMessageHandler))
            {
                BuildHttpClient(client);
                if (!string.IsNullOrEmpty(_token))
                {
                    if (client.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        client.DefaultRequestHeaders.Remove("Authorization");
                    }
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
                }

                var resp = await client.GetAsync(string.Format(Const.DOWNLOAD_FILE, messageId));
                if (resp.IsSuccessStatusCode)
                {
                    var steam = await resp.Content.ReadAsByteArrayAsync();
                    return steam;
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
                    return await DownloadFileAsync(messageId, progressCallBack, excuted);
                }
                else if (resp.StatusCode == HttpStatusCode.BadRequest)
                {
                    var message = await resp.Content.ReadAsStringAsync();
                    await ExcuteWhileBadRequestAsync?.Invoke(message);
                }
            }

            return default;
        }

        public async Task<byte[]> DownloadNetworkImageAsync(string fullPath,
                Action<object, HttpProgressEventArgs> progressCallBack = null, bool excuted = false)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            ProgressMessageHandler progressMessageHandler = new ProgressMessageHandler(httpClientHandler);
            if (progressCallBack != null)
            {
                progressMessageHandler.HttpReceiveProgress += (obj, e) => progressCallBack(obj, e);
            }
            using (var client = new HttpClient(progressMessageHandler))
            {
                BuildHttpClient(client, false);
                if (!string.IsNullOrEmpty(_token))
                {
                    if (client.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        client.DefaultRequestHeaders.Remove("Authorization");
                    }
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
                }

                var resp = await client.GetAsync(fullPath);
                if (resp.IsSuccessStatusCode)
                {
                    var steam = await resp.Content.ReadAsByteArrayAsync();
                    return steam;
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
                    return await DownloadNetworkImageAsync(fullPath, progressCallBack, excuted);
                }
                else if (resp.StatusCode == HttpStatusCode.BadRequest)
                {
                    var message = await resp.Content.ReadAsStringAsync();
                    await ExcuteWhileBadRequestAsync?.Invoke(message);
                }
            }

            return default;
        }

        public async Task<T> RefreshTokenAsync<T>(dynamic body)
        {
            var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resp = await _httpClient.PostAsync(Const.REFRESH_TOKEN, content);
            if (resp.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), _jsonOptions);
            }

            return default;
        }

        public Func<Task<bool>> TryRefreshTokenAsync { get; set; } //当服务端返回401的时候，尝试利用refreshtoken重新获取accesstoken以及refreshtoken
        public Func<Task> ExcuteWhileUnauthorizedAsync { get; set; } //401
        public Func<string, Task> ExcuteWhileBadRequestAsync { get; set; } //400

        private void BuildHttpClient(HttpClient httpClient, bool api = true)
        {
            if (api)
            {
                httpClient.BaseAddress = new Uri($"http://{_contextAPIOptions.Host}:{_contextAPIOptions.Port}/api/");
            }
            httpClient.Timeout = TimeSpan.FromSeconds(600); //Test
            httpClient.DefaultRequestVersion = HttpVersion.Version10;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}

using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Shared.Application.Contract.Services;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Configurations;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.Application.Impl.Services
{
    public class ContextAPIGrpcService : IContextAPIGrpcService, IAppService, IPreheatService
    {
        private SocketServerService.SocketServerServiceClient _serverServiceClient;
        private readonly ContextAPIOptions _contextAPIOptions;

        public ContextAPIGrpcService(IOptions<ContextAPIOptions> options)
        {
            _contextAPIOptions = options.Value;
        }

        public Task PreheatAsync()
        {
            var channel = GrpcChannel.ForAddress($"http://{_contextAPIOptions.Host}:{_contextAPIOptions.Port}", new GrpcChannelOptions()
            {
                HttpHandler = new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip,
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                },
                MaxRetryAttempts = _contextAPIOptions.MaxRetryAttempts,
                Credentials = ChannelCredentials.Insecure, //不使用安全连接
            });
            _serverServiceClient = new SocketServerService.SocketServerServiceClient(channel);

            return Task.CompletedTask;
        }

        public async Task<SocketServerUserLoginResponse> SocketServerUserLoginAsync(SocketServerUserLoginRequest request)
        {
            var headers = new Metadata
            {
                { "Authorization", $"Bearer {request.AccessToken}" }
            };

            return await _serverServiceClient.SocketServerUserLoginAsync(request, headers);
        }
    }
}

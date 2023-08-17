using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using ThreeL.Shared.Application.Contract.Services;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Configurations;
using ThreeL.SocketServer.Application.Contract.Interceptors;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.Application.Impl.Services
{
    public class ContextAPIGrpcService : IContextAPIGrpcService, IPreheatService
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

        [GrpcException]
        public async Task<SocketServerUserLoginResponse> SocketServerUserLoginAsync(SocketServerUserLoginRequest request, string token)
        {
            return await _serverServiceClient.SocketServerUserLoginAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        [GrpcException]
        public async Task<FileInfoResponse> FetchFileInfoAsync(FileInfoRequest request, string token)
        {
            return await _serverServiceClient.FetchFileInfoAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        [GrpcException]
        public async Task<ChatRecordPostResponse> PostChatRecordsAsync(IEnumerable<ChatRecordPostRequest> requests, string token)
        {
            var call = _serverServiceClient.PostChatRecord(new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
            var stream = call.RequestStream;
            foreach (var request in requests)
            {
                await stream.WriteAsync(request);
            }

            await stream.CompleteAsync();
            return await call.ResponseAsync;
        }

        [GrpcException]
        public async Task<ChatRecordPostResponse> PostChatRecordAsync(ChatRecordPostRequest request, string token)
        {
            return await _serverServiceClient.PostChatRecordSingleAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        [GrpcException]
        public async Task<ChatRecordWithdrawResponse> WithdrawChatRecordAsync(ChatRecordWithdrawRequest request, string token)
        {
            return await _serverServiceClient.WithdrawChatRecordAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }
    }
}

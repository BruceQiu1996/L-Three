﻿using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using ThreeL.Shared.Application.Contract.Services;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Configurations;
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

        public async Task<SocketServerUserLoginResponse> SocketServerUserLoginAsync(SocketServerUserLoginRequest request, string token)
        {
            return await _serverServiceClient.SocketServerUserLoginAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<FileInfoResponse> FetchFileInfoAsync(FileInfoRequest request, string token)
        {
            return await _serverServiceClient.FetchFileInfoAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

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

        public async Task<ChatRecordPostResponse> PostChatRecordAsync(ChatRecordPostRequest request, string token)
        {
            return await _serverServiceClient.PostChatRecordSingleAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<ChatRecordWithdrawResponse> WithdrawChatRecordAsync(ChatRecordWithdrawRequest request, string token)
        {
            return await _serverServiceClient.WithdrawChatRecordAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<AddFriendResponse> AddFriendAsync(AddFriendRequest request, string token)
        {
            return await _serverServiceClient.AddFriendAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<ReplyAddFriendResponse> ReplyAddFriendAsync(ReplyAddFriendRequest request, string token)
        {
            return await _serverServiceClient.ReplyAddFriendAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<InviteFriendsIntoGroupResponse> InviteFriendsIntoGroupAsync(InviteFriendsIntoGroupRequest request, string token)
        {
            return await _serverServiceClient.InviteFriendsIntoGroupAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<ValidateRelationResponse> ValidateRelation(ValidateRelationRequest request, string token)
        {
            return await _serverServiceClient.ValidateRelationAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<VoiceChatRecordPostResponse> PostVoiceChatRecordAsync(VoiceChatRecordPostRequest request, string token)
        {
            return await _serverServiceClient.PostVoiceChatRecordSingleAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<VoiceChatRecorStatusResponse> GetVoiceChatStatus(VoiceChatRecorStatusRequest request, string token)
        {
            return await _serverServiceClient.GetVoiceChatStatusAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<VoiceChatRecorStatusUpdateResponse> UpdateVoiceChatStatus(VoiceChatRecorStatusUpdateRequest request, string token)
        {
            return await _serverServiceClient.UpdateVoiceChatStatusAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }

        public async Task<VoiceChatRecorStatusUpdateResponse> FinishVoiceChat(VoiceChatRecordFinishRequest request, string token)
        {
            return await _serverServiceClient.FinishVoiceChatStatusAsync(request, new Metadata()
            {
                { "Authorization", $"Bearer {token}" }
            });
        }
    }
}

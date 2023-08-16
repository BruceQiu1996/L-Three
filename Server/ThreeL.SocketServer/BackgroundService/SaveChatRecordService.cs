using System.Threading.Channels;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.BackgroundService
{
    public class SaveChatRecordService
    {
        private readonly ChannelWriter<ChatRecordPostRequest> _writeChannel;
        private readonly ChannelReader<ChatRecordPostRequest> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IContextAPIGrpcService _contextAPIGrpcService;

        public SaveChatRecordService(IContextAPIGrpcService contextAPIGrpcService)
        {
            var channelOptions = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };

            _contextAPIGrpcService = contextAPIGrpcService;
            var channel = Channel.CreateBounded<ChatRecordPostRequest>(channelOptions);
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageRecordCustomer readOperateLogService = new MessageRecordCustomer(_readChannel, _contextAPIGrpcService);
            var _ = Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~SaveChatRecordService()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteRecordAsync(ChatRecordPostRequest chatRecord)
        {
            await _writeChannel.WriteAsync(chatRecord);
        }

        public class MessageRecordCustomer
        {
            private readonly ChannelReader<ChatRecordPostRequest> _readChannel;
            private readonly IContextAPIGrpcService _contextAPIGrpcService;

            public MessageRecordCustomer(ChannelReader<ChatRecordPostRequest> readChannel, IContextAPIGrpcService contextAPIGrpcService)
            {
                _readChannel = readChannel;
                _contextAPIGrpcService = contextAPIGrpcService;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                while (await _readChannel.WaitToReadAsync(cancellationToken))
                {
                    var entities = new List<ChatRecordPostRequest>();
                    //多消费下，需要考虑并发问题
                    foreach (var index in Enumerable.Range(0, _readChannel.Count))
                    {
                        entities.Add(await _readChannel.ReadAsync());
                    }

                    //await _contextAPIGrpcService.PostChatRecordsAsync(entities); TODO等用上再传入access token
                    if (cancellationToken.IsCancellationRequested) break;
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}

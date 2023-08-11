using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Entities;

namespace ThreeL.Client.Win.BackgroundService
{
    public class SaveChatRecordService
    {
        private readonly ChannelWriter<ChatRecord> _writeChannel;
        private readonly ChannelReader<ChatRecord> _readChannel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public SaveChatRecordService(ClientSqliteContext clientSqliteContext)
        {
            var channelOptions = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            };

            var channel = Channel.CreateBounded<ChatRecord>(channelOptions);
            _writeChannel = channel.Writer;
            _readChannel = channel.Reader;
            MessageRecordCustomer readOperateLogService = new MessageRecordCustomer(_readChannel, clientSqliteContext);
            var _ = Task.Run(async () => await readOperateLogService.StartAsync(_cancellationTokenSource.Token));
        }

        ~SaveChatRecordService()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task WriteLogAsync(ChatRecord chatRecord)
        {
            await _writeChannel.WriteAsync(chatRecord);
        }

        public class MessageRecordCustomer
        {
            private readonly ChannelReader<ChatRecord> _readChannel;
            private readonly ClientSqliteContext _clientSqliteContext;

            public MessageRecordCustomer(ChannelReader<ChatRecord> readChannel, ClientSqliteContext clientSqliteContext)
            {
                _readChannel = readChannel;
                _clientSqliteContext = clientSqliteContext;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                while (await _readChannel.WaitToReadAsync(cancellationToken))
                {
                    var entities = new List<ChatRecord>();
                    //多消费下，需要考虑并发问题
                    foreach (var index in Enumerable.Range(0, _readChannel.Count))
                    {
                        entities.Add(await _readChannel.ReadAsync());
                    }

                    await HandleMessageIntoSqliteDatabaseAsync(entities);
                    if (cancellationToken.IsCancellationRequested) break;
                }
            }

            private async Task HandleMessageIntoSqliteDatabaseAsync(IEnumerable<ChatRecord> messages)
            {
                await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection,
                    "INSERT INTO ChatRecord VALUES(@MessageId,@Message,@MessageRecordType,@ImageType,@SendTime,@From,@To,@ResourceLocalLocation,@FileId,@ResourceSize,@Tag1,@Tag2,@Tag3)", messages);
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}

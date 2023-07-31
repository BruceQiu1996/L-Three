using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Utils;
using ThreeL.Shared.SuperSocket.Cache;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public AsyncRelayCommand LoadCommandAsync { get; set; }

        private readonly PacketWaiter _packetWaiter;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly SocketServerOptions _socketServerOptions;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly TcpSuperSocketClient _tcpSuperSocket; //通讯服务器socket
        private readonly UdpSuperSocketClient _udpSuperSocket; //本地udp通讯socket

        public MainWindowViewModel(TcpSuperSocketClient tcpSuperSocket,
                                   UdpSuperSocketClient udpSuperSocket,
                                   ClientSqliteContext clientSqliteContext,
                                   IOptions<SocketServerOptions> socketServerOptions,
                                   SequenceIncrementer sequenceIncrementer,
                                   PacketWaiter packetWaiter)
        {
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            _packetWaiter = packetWaiter;
            _sequenceIncrementer = sequenceIncrementer;
            _clientSqliteContext = clientSqliteContext;
            _socketServerOptions = socketServerOptions.Value;
            _tcpSuperSocket = tcpSuperSocket;
            _udpSuperSocket = udpSuperSocket;
        }

        private async Task LoadAsync() 
        {
            try
            {
                var result = await ConnectServerAsync();
                if(!result)
                    throw new Exception("连接服务器失败");

                _tcpSuperSocket.mClient.StartReceive();
                var packet = new Packet<LoginCommand>()
                {
                    Checkbit = 8240,
                    Sequence = _sequenceIncrementer.GetNextSequence(),
                    MessageType = MessageType.Login,
                    Body = new LoginCommand
                    {
                        UserId = App.UserProfile.Id,
                        AccessToken = App.UserProfile.AccessToken
                    }
                };

                //need answer
                _packetWaiter.AddWaitPacket($"answer:{packet.Sequence}",null,false);
                await _tcpSuperSocket.SendBytes(packet.Serialize());
                var answer = 
                    await _packetWaiter.GetAnswerPacketAsync($"answer:{packet.Sequence}", 
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)));
                if (answer == null) 
                {
                    throw new Exception("登录聊天服务器超时");
                }
            }
            catch (Exception ex) 
            {
                
            }
        }

        private async Task<bool> ConnectServerAsync()
        {
            return await _tcpSuperSocket.ConnectAsync(_socketServerOptions.Host,_socketServerOptions.Port);
        }
    }
}

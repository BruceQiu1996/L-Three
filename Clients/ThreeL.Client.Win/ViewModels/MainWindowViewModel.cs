using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public AsyncRelayCommand LoadCommandAsync { get; set; }

        private readonly SocketServerOptions _socketServerOptions;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly TcpSuperSocketClient _tcpSuperSocket; //通讯服务器socket
        private readonly UdpSuperSocketClient _udpSuperSocket; //本地udp通讯socket

        public MainWindowViewModel(TcpSuperSocketClient tcpSuperSocket,
                                   UdpSuperSocketClient udpSuperSocket,
                                   ClientSqliteContext clientSqliteContext,
                                   IOptions<SocketServerOptions> socketServerOptions)
        {
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
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
                await _tcpSuperSocket.SendBytes(new Packet<LoginCommand>()
                {
                    Checkbit = 570,
                    Sequence = 250,
                    MessageType = MessageType.Login,
                    Body = new LoginCommand
                    {
                        UserId = App.UserProfile.Id,
                        AccessToken = App.UserProfile.AccessToken
                    }
                }.Serialize());
                
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

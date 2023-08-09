using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.Pages;
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
        private readonly GrowlHelper _growlHelper;
        private readonly ContextAPIService _contextAPIService;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly SocketServerOptions _socketServerOptions;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly TcpSuperSocketClient _tcpSuperSocket; //通讯服务器socket
        private readonly UdpSuperSocketClient _udpSuperSocket; //本地udp通讯socket
        private readonly Page _chatPage;
        
        private Page _currentPage;
        public Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public MainWindowViewModel(TcpSuperSocketClient tcpSuperSocket,
                                   GrowlHelper growlHelper,
                                   ContextAPIService contextAPIService,
                                   UdpSuperSocketClient udpSuperSocket,
                                   ClientSqliteContext clientSqliteContext,
                                   IOptions<SocketServerOptions> socketServerOptions,
                                   SequenceIncrementer sequenceIncrementer,
                                   Chat chatPage,
                                   PacketWaiter packetWaiter)
        {
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            _growlHelper = growlHelper;
            _packetWaiter = packetWaiter;
            _contextAPIService = contextAPIService;
            _sequenceIncrementer = sequenceIncrementer;
            _clientSqliteContext = clientSqliteContext;
            _socketServerOptions = socketServerOptions.Value;
            _tcpSuperSocket = tcpSuperSocket;
            _udpSuperSocket = udpSuperSocket;
            _chatPage = chatPage;
        }

        private async Task LoadAsync() 
        {
            try
            {
                var result = await ConnectServerAsync();
                if (!result)
                    throw new Exception("连接服务器失败");

                _tcpSuperSocket.mClient.StartReceive();
                var packet = new Packet<LoginCommand>()
                {
                    Checkbit = 8240,
                    Sequence = _sequenceIncrementer.GetNextSequence(),
                    MessageType = MessageType.Login,
                    Body = new LoginCommand
                    {
                        UserId = App.UserProfile.UserId,
                        AccessToken = App.UserProfile.AccessToken
                    }
                };

                //need answer
                _packetWaiter.AddWaitPacket($"answer:{packet.Sequence}", null, false);
                await _tcpSuperSocket.SendBytes(packet.Serialize());
                var answer =
                    await _packetWaiter.GetAnswerPacketAsync<Packet<LoginCommandResponse>>($"answer:{packet.Sequence}");
                if (answer == null || !answer.Body.Result)
                {
                    throw new Exception("登录聊天服务器超时");
                }
                App.UserProfile.SocketAccessToken = answer.Body.SsToken;
                CurrentPage = _chatPage;
            }
            catch (Exception ex) 
            {
                await _tcpSuperSocket.CloseConnectAsync();
                _growlHelper.Warning(ex.Message);
            }
        }

        private async Task<bool> ConnectServerAsync()
        {
            return await _tcpSuperSocket.ConnectAsync(_socketServerOptions.Host,_socketServerOptions.Port);
        }
    }
}

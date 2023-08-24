using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using SuperSocket.Channel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly Chat _chatPage;
        private readonly Setting _setting;
        private readonly Apply _apply;

        private Page _currentPage;
        public Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private string _tips;
        public string Tips
        {
            get => _tips;
            set => SetProperty(ref _tips, value);
        }

        public MainWindowViewModel(TcpSuperSocketClient tcpSuperSocket,
                                   GrowlHelper growlHelper,
                                   ContextAPIService contextAPIService,
                                   UdpSuperSocketClient udpSuperSocket,
                                   ClientSqliteContext clientSqliteContext,
                                   IOptions<SocketServerOptions> socketServerOptions,
                                   SequenceIncrementer sequenceIncrementer,
                                   Chat chatPage,
                                   Apply apply,
                                   Setting setting,
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
            _setting = setting;
            _apply = apply;
            _tcpSuperSocket.DisConnectionEvent += DisConnectionCallbackAsync;
            _tcpSuperSocket.ConnectedEvent += ConnectedCallback;

            WeakReferenceMessenger.Default.Register<MainWindowViewModel, string, string>(this, "switch-page",
                (x, y) =>
                {
                    CurrentPage = y switch
                    {
                        "chat" => _chatPage,
                        "setting" => _setting,
                        "apply" => _apply,
                        _ => _chatPage
                    };
                });
        }

        private void ConnectedCallback()
        {
            Tips = null;
        }

        private async Task DisConnectionCallbackAsync(CloseEventArgs args)
        {
            var message = args.Reason switch
            {
                CloseReason.ServerShutdown => "服务器已关闭",
                CloseReason.LocalClosing => "本地关闭",
                CloseReason.RemoteClosing => "远程关闭",
                CloseReason.SocketError => "Socket错误",
                CloseReason.ProtocolError => "协议错误",
                CloseReason.ApplicationError => "业务出错",
                CloseReason.TimeOut => "超时",
                CloseReason.InternalError => "网络问题",
                CloseReason.Unknown => "未知",
                _ => "未知",
            };

            await Task.Yield();
            foreach (var index in Enumerable.Range(1, 60))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Tips = $"连接断开，原因：{message}，正在第{index}次重连...";
                });
                if (await ConnectServerAsync(1))
                    break;
            }

            if (!_tcpSuperSocket.Connected)
            {
                _growlHelper.Warning("与服务器的连接出现严重故障，软件即将退出");
                Application.Current.Shutdown();
            }
            else
            {
                await HandShakeAfterSocketConnectedAsync();
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                var result = await ConnectServerAsync();
                if (!result)
                    throw new Exception("连接服务器失败");

                await HandShakeAfterSocketConnectedAsync();
                CurrentPage = _chatPage; //TODO
            }
            catch (Exception ex)
            {
                await _tcpSuperSocket.CloseConnectAsync();
                _growlHelper.Warning(ex.Message);
            }
        }

        private async Task HandShakeAfterSocketConnectedAsync()
        {
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
            _packetWaiter.AddWaitPacket($"answer:{packet.Sequence}", null, false);
            var sendResult = await _tcpSuperSocket.SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                throw new Exception("登录聊天服务器失败");
            }
            var answer =
                await _packetWaiter.GetAnswerPacketAsync<Packet<LoginCommandResponse>>($"answer:{packet.Sequence}");
            if (answer == null || !answer.Body.Result)
            {
                throw new Exception("登录聊天服务器超时");
            }

            App.UserProfile.SocketAccessToken = answer.Body.SsToken;
        }

        private async Task<bool> ConnectServerAsync(int retryTimes = 3)
        {
            return await _tcpSuperSocket.ConnectAsync(_socketServerOptions.Host, _socketServerOptions.Port, retryTimes);
        }
    }
}

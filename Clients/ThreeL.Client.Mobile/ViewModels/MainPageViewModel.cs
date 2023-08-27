using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using ThreeL.Client.Mobile.Helper;
using ThreeL.Client.Mobile.Pages.Content;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Utils;
using ThreeL.Shared.SuperSocket.Cache;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Mobile.ViewModels
{
    public class MainPageViewModel : ObservableObject 
    {
        public ObservableCollection<Page> Pages { get; set; }

        private Page _currentPage;
        public Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private readonly TcpSuperSocketClient _tcpSuperSocket;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly PacketWaiter _packetWaiter;
        private readonly SocketServerOptions _socketServerOptions;
        private readonly WarningHelper _warningHelper;

        public AsyncRelayCommand LoadedCommandAsync { get; set; }
        public MainPageViewModel(TcpSuperSocketClient tcpSuperSocket, 
                                 SequenceIncrementer sequenceIncrementer,
                                 PacketWaiter packetWaiter,
                                 Chat chat,
                                 Setting setting,
                                 IOptions<SocketServerOptions> socketServerOptions, 
                                 WarningHelper warningHelper
                                 )
        {
            _tcpSuperSocket = tcpSuperSocket;
            _warningHelper = warningHelper;
            _socketServerOptions = socketServerOptions.Value;
            _packetWaiter = packetWaiter;
            _sequenceIncrementer =  sequenceIncrementer;
            LoadedCommandAsync = new AsyncRelayCommand(Loaded);
            Pages = new ObservableCollection<Page>() { chat, setting };
        }

        private async Task Loaded() 
        {
            try
            {
                var result = await ConnectServerAsync();
                if (!result)
                    throw new Exception("连接服务器失败");

                await HandShakeAfterSocketConnectedAsync();
                CurrentPage = Pages[0];
            }
            catch (Exception ex)
            {
                await _tcpSuperSocket.CloseConnectAsync();
                await _warningHelper.Warning(ex.Message);
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

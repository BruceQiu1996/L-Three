using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.MyControls;
using ThreeL.Shared.SuperSocket.Cache;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Metadata;
using TextMessage = ThreeL.Shared.SuperSocket.Dto.Message.TextMessage;

namespace ThreeL.Client.Win.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        public RelayCommand OpenEmojiCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SelectFriendCommandAsync { get; set; }
        public AsyncRelayCommand<SelectEmojiClickRoutedEventArgs> AddEmojiCommandAsync { get; set; }
        public AsyncRelayCommand SendMessageCommandAsync { get; set; }

        private ObservableCollection<FriendViewModel> friendViewModels;
        public ObservableCollection<FriendViewModel> FriendViewModels
        {
            get => friendViewModels;
            set => SetProperty(ref friendViewModels, value);
        }

        private FriendViewModel friendViewModel;
        public FriendViewModel FriendViewModel
        {
            get => friendViewModel;
            set => SetProperty(ref friendViewModel, value);
        }

        private string textMessage;
        public string TextMessage
        {
            get => textMessage;
            set => SetProperty(ref textMessage, value);
        }

        private bool _isEmojiOpen;
        public bool IsEmojiOpen
        {
            get => _isEmojiOpen;
            set => SetProperty(ref _isEmojiOpen, value);
        }

        private readonly ContextAPIService _contextAPIService;
        private readonly GrowlHelper _growlHelper;
        private readonly UdpSuperSocketClient _udpSuperSocketClient;
        private readonly PacketWaiter _packetWaiter;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly TcpSuperSocketClient _tcpSuperSocketClient;
        private readonly IPEndPoint _iPEndPoint;

        public ChatViewModel(ContextAPIService contextAPIService,
                             GrowlHelper growlHelper,
                             PacketWaiter packetWaiter,
                             IPEndPoint iPEndPoint,
                             UdpSuperSocketClient udpSuperSocketClient,
                             SequenceIncrementer sequenceIncrementer,
                             TcpSuperSocketClient tcpSuperSocketClient)
        {
            _contextAPIService = contextAPIService;
            _growlHelper = growlHelper;
            _packetWaiter = packetWaiter;
            _iPEndPoint = iPEndPoint;
            _udpSuperSocketClient = udpSuperSocketClient;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            SelectFriendCommandAsync = new AsyncRelayCommand(SelectFriendAsync);
            SendMessageCommandAsync = new AsyncRelayCommand(SendMessageAsync);
            AddEmojiCommandAsync = new AsyncRelayCommand<SelectEmojiClickRoutedEventArgs>(AddEmojiAsync);
            OpenEmojiCommand = new RelayCommand(OpenEmoji);
            _sequenceIncrementer = sequenceIncrementer;
            _tcpSuperSocketClient = tcpSuperSocketClient;
        }

        private async Task LoadAsync()
        {
            //TODO删除代码
            try
            {
                //获取好友列表
                var resp = await _contextAPIService.GetAsync<IEnumerable<FriendDto>>("relations/friends");
                if (resp != null)
                {
                    List<FriendViewModel> friends = new List<FriendViewModel>();
                    //处理得到好友列表
                    foreach (var dto in resp)
                    {
                        if (dto.ActiverId == App.UserProfile.UserId)
                        {
                            friends.Add(new FriendViewModel
                            {
                                Id = dto.PassiverId,
                                UserName = dto.PassiverName,
                                Remark = dto.PassiverRemark
                            });
                        }
                        else if (dto.PassiverId == App.UserProfile.UserId)
                        {
                            friends.Add(new FriendViewModel
                            {
                                Id = dto.ActiverId,
                                UserName = dto.ActiverName,
                                Remark = dto.ActiverRemark
                            });
                        }
                    }
                    //加载好友列表
                    FriendViewModels = new ObservableCollection<FriendViewModel>(friends); FriendViewModels.Insert(0, new FriendViewModel()
                    {
                        Id = App.UserProfile.UserId,
                        Remark = "我"
                    });
                }
            }
            catch (Exception ex)
            {
                _growlHelper.Warning(ex.Message);
            }
        }

        /// <summary>
        /// 选中一个好友的时候进行连接
        /// </summary>
        /// <returns></returns>
        private async Task SelectFriendAsync()
        {
            if (FriendViewModel.Hosts.Count <= 0)
            {
                var packet = new Packet<RequestForUserEndpointCommand>()
                {
                    Checkbit = 8240,
                    Sequence = _sequenceIncrementer.GetNextSequence(),
                    MessageType = MessageType.RequestForUserEndpoint,
                    Body = new RequestForUserEndpointCommand
                    {
                        UserId = FriendViewModel.Id,
                        SsToken = App.UserProfile.SocketAccessToken
                    }
                };

                _packetWaiter.AddWaitPacket($"answer:{packet.Sequence}", null, false);
                await _tcpSuperSocketClient.SendBytes(packet.Serialize());
                var answer =
                    await _packetWaiter.GetAnswerPacketAsync<Packet<RequestForUserEndpointCommandResponse>>($"answer:{packet.Sequence}");

                if (answer != null && answer.Body.Result && !string.IsNullOrEmpty(answer.Body.Addresses))
                {
                    var addrs = answer.Body.Addresses.Split(",");
                    FriendViewModel.Hosts.AddRange(addrs);
                }
            }
        }
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(TextMessage))
                return;

            var packet = new Packet<TextMessage>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.Text,
                Body = new TextMessage
                {
                    SendTime = DateTime.Now,
                    From = App.UserProfile.UserId,
                    To = FriendViewModel.Id,
                    Text = textMessage
                }
            };

            foreach (var endpoint in FriendViewModel.Hosts)
            {
                await _udpSuperSocketClient.SendBytes(IPEndPoint.Parse(endpoint), packet.Serialize());
            }
        }

        private async Task AddEmojiAsync(SelectEmojiClickRoutedEventArgs routedEventArgs) 
        {
            IsEmojiOpen = false;
        }

        private void OpenEmoji() 
        {
            IsEmojiOpen = true;
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.Pages;
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
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SelectFriendCommandAsync { get; set; }
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

        private FlowDocument _sendRichMessage;
        public FlowDocument SendRichMessage
        {
            get => _sendRichMessage;
            set => SetProperty(ref _sendRichMessage, value);
        }

        private readonly ContextAPIService _contextAPIService;
        private readonly GrowlHelper _growlHelper;
        private readonly UdpSuperSocketClient _udpSuperSocketClient;
        private readonly PacketWaiter _packetWaiter;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly TcpSuperSocketClient _tcpSuperSocketClient;
        private readonly IPEndPoint _iPEndPoint;
        private readonly EmojiHelper _emojiHelper;

        public ChatViewModel(ContextAPIService contextAPIService,
                             GrowlHelper growlHelper,
                             PacketWaiter packetWaiter,
                             IPEndPoint iPEndPoint,
                             EmojiHelper emojiHelper,
                             UdpSuperSocketClient udpSuperSocketClient,
                             SequenceIncrementer sequenceIncrementer,
                             TcpSuperSocketClient tcpSuperSocketClient)
        {
            _contextAPIService = contextAPIService;
            _growlHelper = growlHelper;
            _packetWaiter = packetWaiter;
            _iPEndPoint = iPEndPoint;
            _emojiHelper = emojiHelper;
            _udpSuperSocketClient = udpSuperSocketClient;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            SelectFriendCommandAsync = new AsyncRelayCommand(SelectFriendAsync);
            SendMessageCommandAsync = new AsyncRelayCommand(SendMessageAsync);
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
            var textMessage = GetSendMessage(App.ServiceProvider.GetRequiredService<Chat>().rtb.Document);
            if (string.IsNullOrEmpty(textMessage))
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
        private string GetSendMessage(FlowDocument fld)
        {
            if (fld == null)
            {
                return string.Empty;
            }
            string resutStr = string.Empty;
            foreach (var root in fld.Blocks)
            {
                foreach (var item in ((Paragraph)root).Inlines)
                {
                    //如果是Emoji则进行转换
                    if (item is InlineUIContainer)
                    {
                        System.Windows.Controls.Image img = (System.Windows.Controls.Image)((InlineUIContainer)item).Child;
                        resutStr += GetEmojiName(img.Source.ToString());
                    }
                    //如果是文本，则直接赋值
                    if (item is Run)
                    {
                        resutStr += ((Run)item).Text;
                    }
                }
            }

            return resutStr;
        }

        private string GetEmojiName(string str)
        {
            foreach (KeyValuePair<string, BitmapImage> item in _emojiHelper.EmojiCode)
            {
                if (item.Value.ToString().Equals(str))
                {
                    return item.Key;
                }
            }
            return string.Empty;
        }

        private void StrToFlDoc(string str, FlowDocument fld, Paragraph par)
        {
            //当递归结束以后，也就是长度为0的时候，就跳出
            if (str.Length <= 0)
            {
                fld.Blocks.Add(par);
                return;
            }
            //如果字符串里不存在[时，则直接添加内容
            if (!str.Contains('['))
            {
                par.Inlines.Add(new Run(str));
                str = str.Remove(0, str.Length);
                StrToFlDoc(str, fld, par);
            }
            else
            {
                //首先判断第一位是不是[，如果是，则证明是表情，用正则获取表情，然后将字符串长度进行移除，递归
                if (str[0].Equals('['))
                {
                    par.Inlines.Add(new InlineUIContainer(new System.Windows.Controls.Image { Width = 20, Height = 20, 
                        Source = _emojiHelper.EmojiCode[GetEmojiNameByRegex(str)] }));
                    str = str.Remove(0, GetEmojiNameByRegex(str).Length);
                    StrToFlDoc(str, fld, par);
                }
                else
                {//如果第一位不是[的话，则是字符串，直接添加进去
                    par.Inlines.Add(new Run(GetTextByRegex(str)));
                    str = str.Remove(0, GetTextByRegex(str).Length);
                    StrToFlDoc(str, fld, par);
                }
            }
        }

        private string GetEmojiNameByRegex(string str)
        {
            string name = Regex.Match(str, "(?<=\\[).*?(?=\\])").Value;
            return "[" + name + "]";
        }
        /// <summary>
        /// 获取文本信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetTextByRegex(string str)
        {
            string text = Regex.Match(str, "^.*?(?=\\[)").Value;
            return text;
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Handlers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.BackgroundService;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.MyControls;
using ThreeL.Client.Win.ViewModels.Messages;
using ThreeL.Infra.Core.Cryptography;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.SuperSocket.Cache;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        public static DateTime INITIALIZE_TIME;//初始化时间，后期用于同步消息，防止消息丢失
        public RelayCommand GotoApplyPageCommand { get; set; }
        public RelayCommand CutScreenshotCommand { get; set; }
        public RelayCommand OpenEmojiCommand { get; set; }
        public RelayCommand<ScrollChangedEventArgs> ChatScrollChangeCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SelectFriendCommandAsync { get; set; }
        public AsyncRelayCommand<SelectEmojiClickRoutedEventArgs> AddEmojiCommandAsync { get; set; }
        public AsyncRelayCommand SendMessageCommandAsync { get; set; }
        public AsyncRelayCommand ChooseFileSendCommandAsync { get; set; }
        public AsyncRelayCommand<KeyEventArgs> SendTextboxKeyDownCommandAsync { get; set; }
        public RelayCommand GotoSettingsPageCommand { get; set; }
        public AsyncRelayCommand DisplayDetailCommand { get; set; }
        public AsyncRelayCommand StartVoiceChatCommandAsync { get; set; }

        private ObservableCollection<RelationViewModel> relationViewModels;
        public ObservableCollection<RelationViewModel> RelationViewModels
        {
            get => relationViewModels;
            set => SetProperty(ref relationViewModels, value);
        }

        private RelationViewModel relationViewModel;
        public RelationViewModel RelationViewModel
        {
            get => relationViewModel;
            set
            {
                InitScrollFlags();
                SetProperty(ref relationViewModel, value);
                if (value != null)
                {
                    value.UnReadCount = 0;
                }
            }
        }

        private DetailWindowViewModel detailWindowViewModel;
        public DetailWindowViewModel DetailWindowViewModel
        {
            get => detailWindowViewModel;
            set => SetProperty(ref detailWindowViewModel, value);
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

        private bool _isUserDetailOpen;
        public bool IsUserDetailOpen
        {
            get => _isUserDetailOpen;
            set => SetProperty(ref _isUserDetailOpen, value);
        }

        private int needProcessApplyCounts;
        public int NeedProcessApplyCounts
        {
            get => needProcessApplyCounts;
            set => SetProperty(ref needProcessApplyCounts, value);
        }

        private UserProfile userProfile;
        public UserProfile UserProfile
        {
            get { return userProfile; }
            set => SetProperty(ref userProfile, value);
        }

        private BitmapImage _avatar;
        public BitmapImage Avatar
        {
            get { return _avatar; }
            set => SetProperty(ref _avatar, value);
        }

        private VoiceChatWindow _voiceChatWindow;
        private readonly ContextAPIService _contextAPIService;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        private readonly UdpSuperSocketClient _udpSuperSocketClient;
        private readonly PacketWaiter _packetWaiter;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly TcpSuperSocketClient _tcpSuperSocketClient;
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly MessageFileLocationMapper _messageFileLocationMapper;
        private readonly ILogger _logger;

        [DllImport("PrScrn.dll", EntryPoint = "PrScrn")]
        public extern static int PrScrn();

        public ChatViewModel(ContextAPIService contextAPIService,
                             ClientSqliteContext clientSqliteContext,
                             SaveChatRecordService saveChatRecordService,
                             GrowlHelper growlHelper,
                             FileHelper fileHelper,
                             PacketWaiter packetWaiter,
                             UdpSuperSocketClient udpSuperSocketClient,
                             SequenceIncrementer sequenceIncrementer,
                             TcpSuperSocketClient tcpSuperSocketClient,
                             MessageFileLocationMapper messageFileLocationMapper,
                             ILoggerFactory loggerFactory)
        {
            _contextAPIService = contextAPIService;
            _clientSqliteContext = clientSqliteContext;
            _saveChatRecordService = saveChatRecordService;
            _growlHelper = growlHelper;
            _packetWaiter = packetWaiter;
            _fileHelper = fileHelper;
            _udpSuperSocketClient = udpSuperSocketClient;
            _logger = loggerFactory.CreateLogger(nameof(Module.CLIENT_WIN));
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            //SelectFriendCommandAsync = new AsyncRelayCommand(SelectFriendAsync);
            SendMessageCommandAsync = new AsyncRelayCommand(SendTextMessageAsync);
            AddEmojiCommandAsync = new AsyncRelayCommand<SelectEmojiClickRoutedEventArgs>(SendEmojiAsync);
            OpenEmojiCommand = new RelayCommand(OpenEmoji);
            ChooseFileSendCommandAsync = new AsyncRelayCommand(ChooseFileSendAsync);
            SendTextboxKeyDownCommandAsync = new AsyncRelayCommand<KeyEventArgs>(SendTextboxKeyDownAsync);
            CutScreenshotCommand = new RelayCommand(CutScreenshot);
            GotoSettingsPageCommand = new RelayCommand(GotoSettingsPage);
            GotoApplyPageCommand = new RelayCommand(GotoApplyPage);
            DisplayDetailCommand = new AsyncRelayCommand(DisplayDetailAsync);
            ChatScrollChangeCommand = new RelayCommand<ScrollChangedEventArgs>(ChatScrollChange);
            StartVoiceChatCommandAsync = new AsyncRelayCommand(StartVoiceChatAsync);
            _sequenceIncrementer = sequenceIncrementer;
            _tcpSuperSocketClient = tcpSuperSocketClient;
            _messageFileLocationMapper = messageFileLocationMapper;
            RelationViewModels = new ObservableCollection<RelationViewModel>();

            WeakReferenceMessenger.Default.Register<ChatViewModel, FromToMessageResponse, string>(this, "message-receive",
                async (x, resp) =>
                {
                    var relation = GetRelation(resp.From, resp.To, resp.IsGroup);
                    if (relation == null)
                    {
                        //TODO 存储消息防止因为并发导致消息错乱或者消息丢失
                        return;
                    }

                    //显示错误提示
                    if (!resp.Result)
                    {
                        _growlHelper.Warning(resp.Message);
                    }

                    if (resp is TextMessageResponse)
                    {
                        await ReceiveTextMessageAsync(resp as TextMessageResponse, relation);
                    }
                    else if (resp is ImageMessageResponse)
                    {
                        await ReceiveImageMessageAsync(resp as ImageMessageResponse, relation);
                    }
                    else if (resp is FileMessageResponse)
                    {
                        await ReceiveFileMessageAsync(resp as FileMessageResponse, relation);
                    }
                    else if (resp is VoiceChatMessageResponse)
                    {
                        await ReceiveVoiceChatMessageAsync(resp as VoiceChatMessageResponse, relation);
                    }
                    if (relation != RelationViewModel && resp.From != App.UserProfile.UserId)
                    {
                        relation.UnReadCount++;
                    }

                    if (RelationViewModels.FirstOrDefault() != relation)
                    {
                        var flag = RelationViewModel == relation;
                        RelationViewModels.Remove(relation);
                        RelationViewModels.Insert(0, relation);
                        if (flag)
                        {
                            RelationViewModel = relation;
                        }
                    }
                });

            WeakReferenceMessenger.Default.Register<ChatViewModel, WithdrawMessageResponse, string>(this, "message-withdraw-result",
                (x, resp) =>
                {
                    if (!resp.Result)
                    {
                        _growlHelper.Warning(resp.Message);

                        return;
                    }
                    var relation = GetRelation(resp.From, resp.To, resp.IsGroup);
                    WithdrawMessage(resp, relation);
                });

            WeakReferenceMessenger.Default.Register<ChatViewModel, MessageViewModel, string>(this, "message-resend",
                async (x, y) =>
                {
                    await ResendMessageAsync(y);
                });

            WeakReferenceMessenger.Default.Register<ChatViewModel, MessageViewModel, string>(this, "message-withdraw",
                async (x, y) =>
                {
                    await WithdrawMessageAsync(y);
                });

            WeakReferenceMessenger.Default.Register<ChatViewModel, byte[], string>(this, "avatar-updated",
                (x, y) =>
                {
                    Avatar = _fileHelper.ByteArrayToBitmapImage(y);
                });

            WeakReferenceMessenger.Default.Register<ChatViewModel, string, string>(this, "message-addfriend-apply",
                async (x, y) =>
                {
                    await FetchUserUnProcessApplysAsync();
                });

            WeakReferenceMessenger.Default.Register<ChatViewModel, ReplyAddFriendCommandResponse, string>(this, "message-addfriend-success",
               async (x, y) =>
               {
                   await HandleReplyFriendApplyAsync(y);
               });

            //创建新的群聊事件
            WeakReferenceMessenger.Default.Register<ChatViewModel, GroupCreationResponseDto, string>(this, "message-newgroup",
              async (x, y) =>
              {
                  HandleNewGroup(y);
              });

            //邀请进入群聊
            WeakReferenceMessenger.Default.Register<ChatViewModel, InviteMembersIntoGroupCommandResponse, string>(this, "message-invite-group",
              async (x, y) =>
              {
                  await HandleInviteGroupAsync(y);
              });

            //请求下线
            WeakReferenceMessenger.Default.Register<ChatViewModel, string, string>(this, "message-request-offline",
              async (x, y) =>
              {
                  await OfflineAsync(y);
              });

            //语音通话请求
            WeakReferenceMessenger.Default.Register<ChatViewModel, VoiceChatStatusResponse, string>(this, "message-receive-voicechat-event",
              async (x, y) =>
              {
                  if (!y.Result)
                  {
                      _growlHelper.Warning(y.Message);
                      return;
                  }
                  await HandleVoiceChatEventAsync(y);
              });
        }

        public async Task LoadAsync()
        {
            UserProfile = App.UserProfile;
            if (UserProfile.Avatar != null)
            {
                Avatar = _fileHelper.ByteArrayToBitmapImage(UserProfile.Avatar);
            }
            //TODO删除代码
            try
            {
                INITIALIZE_TIME = DateTime.Now;
                //获取好友列表
                var relations = await _contextAPIService.GetAsync<IEnumerable<RelationDto>>(string.Format(Const.FETCH_RELATIONS,
                    INITIALIZE_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                RelationViewModels.Clear();
                if (relations != null)
                {
                    foreach (var rel in relations.OrderByDescending(x => x.ChatRecords?.FirstOrDefault()?.SendTime))
                    {
                        var fvm = new RelationViewModel
                        {
                            Id = rel.Id,
                            Name = rel.Name,
                            AvatarId = rel.Avatar,
                            Remark = rel.Remark,
                            IsGroup = rel.IsGroup,
                            FetchFirstRecordTime = rel.ChatRecords == null ? null : rel.ChatRecords.Count() < 30 ? null : rel.ChatRecords.LastOrDefault()?.SendTime,
                            FetchLastestRecordTime = INITIALIZE_TIME
                        };
                        if (!fvm.IsGroup)
                        {
                            fvm.TitleDisplayName = string.IsNullOrEmpty(fvm.Remark) ? fvm.Name : fvm.Remark + $" ( {fvm.Name} )";
                        }
                        else
                        {
                            fvm.TitleDisplayName = $"{fvm.Name} ( {rel.MemberCount} ) ";
                        }

                        var messages = await ConvertChatRecordToViewModel(rel.ChatRecords);
                        fvm.AddMessages(messages);
                        RelationViewModels.Add(fvm);
                    }

                    RelationViewModel = RelationViewModels.FirstOrDefault();

                    //加载申请好友记录
                    await FetchUserUnProcessApplysAsync();
                }
            }
            catch (Exception ex)
            {
                _growlHelper.Warning(ex.Message);
                _logger.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// 下线
        /// </summary>
        /// <param name="reason">下线原因</param>
        /// <returns></returns>
        private async Task OfflineAsync(string reason)
        {
            HandyControl.Controls.MessageBox.Show(reason, "系统消息");
            await Task.Delay(5000);
            await App.CloseAsync();
        }

        /// <summary>
        /// 处理语音通话
        /// </summary>
        /// <param name="messageResponse"></param>
        /// <returns></returns>
        private async Task HandleVoiceChatEventAsync(VoiceChatStatusResponse messageResponse)
        {
            var relation = GetRelation(messageResponse.From, messageResponse.To, false);
            if (relation == null)
                return;

            switch (messageResponse.Event)
            {
                case VoiceChatStatus.Initialized:
                    {
                        if (_voiceChatWindow != null && (_voiceChatWindow.DataContext as VoiceChatWindowViewModel).VoiceChatKey != messageResponse.ChatKey)
                        {
                            _voiceChatWindow.Close();
                        }

                        var window = App.ServiceProvider.GetRequiredService<VoiceChatWindow>();
                        var vm = window.DataContext as VoiceChatWindowViewModel;
                        vm.Current = relation;
                        vm.Started = false;
                        vm.VoiceChatKey = messageResponse.ChatKey;
                        vm.WattingText = messageResponse.From == App.UserProfile.UserId ? "正在等待对方接听" : $"{messageResponse.FromName}向您发起语音通话申请";
                        _voiceChatWindow = window;
                        window.Show();
                    }
                    break;
                case VoiceChatStatus.NotAccept:
                case VoiceChatStatus.Canceled:
                case VoiceChatStatus.Rejected:
                case VoiceChatStatus.Finished:
                    {
                        if (_voiceChatWindow != null && _voiceChatWindow.DataContext is VoiceChatWindowViewModel viewModel)
                        {
                            if (viewModel.VoiceChatKey == messageResponse.ChatKey)
                            {
                                _voiceChatWindow.Close();
                                _voiceChatWindow = null;
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 获取用户的申请
        /// </summary>
        /// <returns></returns>
        private async Task FetchUserUnProcessApplysAsync()
        {
            var applys = await _contextAPIService.GetAsync<IEnumerable<FriendApplyResponseDto>>(Const.FETCH_FRIEND_APPLYS);
            if (applys != null)
            {
                IEnumerable<ApplyRecordViewModel> applyRecordViewModels = applys.Select(x => new ApplyRecordViewModel(x)).ToList();
                NeedProcessApplyCounts = applyRecordViewModels.Where(x => !x.FromSelf && x.Status == FriendApplyStatus.TobeProcessed).Count();
                WeakReferenceMessenger.Default.Send<IEnumerable<ApplyRecordViewModel>, string>(applyRecordViewModels, "message-applys");
            }
            else
            {
                NeedProcessApplyCounts = 0;
            }
        }

        private async Task HandleReplyFriendApplyAsync(ReplyAddFriendCommandResponse response)
        {
            if (response.Agree)
            {
                var friend = response.From == App.UserProfile.UserId ? response.To : response.From;
                var name = response.From == App.UserProfile.UserId ? response.ToName : response.FromName;
                var avatar = response.From == App.UserProfile.UserId ? response.ToAvatar : response.FromAvatar;
                var friendViewModel = RelationViewModels.FirstOrDefault(x => x.Id == friend);
                if (friendViewModel == null)
                {
                    RelationViewModels.Insert(0, new RelationViewModel
                    {
                        Id = friend,
                        Name = name,
                        AvatarId = avatar == 0 ? null : avatar,
                        TitleDisplayName = name,
                    });
                }
            }

            await FetchUserUnProcessApplysAsync();
        }

        private void HandleNewGroup(GroupCreationResponseDto group)
        {
            var groupViewModel = new RelationViewModel()
            {
                IsGroup = true,
                Id = group.Id,
                Name = group.Name,
                AvatarId = group.Avatar,
                TitleDisplayName = group.Name,
            };

            groupViewModel.AddMessage(new TipMessageViewModel { Content = "你创建了新的群组,快邀请大家一起聊天吧", SendTime = DateTime.Now });
            RelationViewModels.Insert(0, groupViewModel);
        }

        private async Task HandleInviteGroupAsync(InviteMembersIntoGroupCommandResponse intoGroupCommandResponse)
        {
            var groupViewModel = RelationViewModels.FirstOrDefault(x => x.Id == intoGroupCommandResponse.GroupId);
            var time = DateTime.Now;
            if (groupViewModel == null)
            {
                var vm = new RelationViewModel()
                {
                    IsGroup = true,
                    Id = intoGroupCommandResponse.GroupId,
                    Name = intoGroupCommandResponse.GroupName,
                    AvatarId = intoGroupCommandResponse.GroupAvatar,
                    TitleDisplayName = intoGroupCommandResponse.GroupName,
                    FetchLastestRecordTime = time,
                };
                RelationViewModels.Insert(0, vm);

                //获取群聊的聊天记录
                var records = await _contextAPIService.GetAsync<IEnumerable<ChatRecordResponseDto>>(string.Format(Const.FETCH_RELATION_CHATRECORDS,
                    intoGroupCommandResponse.GroupId, true, time.ToString("yyyy-MM-dd HH:mm:ss.fff")));

                if (records != null)
                {
                    var messages = await ConvertChatRecordToViewModel(records, true);
                    vm.AddMessages(messages, true);
                }
            }
        }

        ///// <summary>
        ///// 选中一个好友或者群组时候进行连接
        ///// </summary>
        ///// <returns></returns>
        //private async Task SelectFriendAsync()
        //{
        //    var tempFriend = FriendViewModel;
        //    if (!tempFriend.LoadedChatRecord)
        //    {
        //        tempFriend.Messages.Clear();
        //        try
        //        {
        //            //加载历史100条聊天记录 //TODO与服务器聊天记录做比较
        //            var records = await FetchChatRecordsFromRemoteAsync(tempFriend.Id, DateTime.Now);
        //            if (records != null && records.Count() > 0)
        //            {
        //                foreach (var record in records)
        //                {
        //                    tempFriend.AddMessage(record);
        //                }
        //            }

        //            tempFriend.LoadedChatRecord = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            //TODO log记录
        //        }
        //    }
        //}

        private void CutScreenshot()
        {
            var result = PrScrn();
            if (result == 1)
            {
                //TODO 立即发送
            }
        }

        private void GotoSettingsPage()
        {
            WeakReferenceMessenger.Default.Send("setting", "switch-page");
        }

        private void GotoApplyPage()
        {
            WeakReferenceMessenger.Default.Send("apply", "switch-page");
        }

        /// <summary>
        /// 点击标题显示详细信息
        /// </summary>
        /// <returns></returns>
        private async Task DisplayDetailAsync()
        {
            var temp = RelationViewModel;
            if (temp == null)
                return;

            if (temp.IsGroup)
            {
                var group = await _contextAPIService.GetAsync<GroupRoughlyDto>(string.Format(Const.GROUP_DETAIL, RelationViewModel.Id));
                if (group == null)
                    return;
                //更新群组信息
                if (temp.AvatarId != group.Avatar)
                {
                    temp.AvatarId = group.Avatar;
                }
                temp.TitleDisplayName = $"{group.Name} ( {group.Users?.Count()} ) ";
                var window = App.ServiceProvider.GetRequiredService<GroupDetailWindow>();
                window.DataContext = new GroupDetailWindowViewModel().FromDto(group);
                window.Owner = App.ServiceProvider.GetRequiredService<MainWindow>();
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();
            }
            else
            {
                var user = await _contextAPIService.GetAsync<UserRoughlyDto>(string.Format(Const.USER, RelationViewModel.Id));
                if (user == null)
                    return;
                if (temp.AvatarId != user.Avatar)
                {
                    temp.AvatarId = user.Avatar;
                }
                DetailWindowViewModel = new UserDetailWindowViewModel().FromDto(user);
                IsUserDetailOpen = true;
            }
        }

        private async Task StartVoiceChatAsync()
        {
            var temp = RelationViewModel;
            if (_voiceChatWindow != null)
            {
                return;
            }

            if (temp == null || temp.IsGroup)
            {
                return;
            }

            ApplyforVoiceChatMessage message = new ApplyforVoiceChatMessage()
            {

                To = temp.Id,
                Platform = "win"
            };

            var packet = new Packet<ApplyforVoiceChatMessage>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.ApplyVoiceChat,
                Body = message
            };

            var sendResult = await _tcpSuperSocketClient.SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                _growlHelper.Warning("网络异常");
            }
        }

        private async Task SendTextboxKeyDownAsync(KeyEventArgs e)
        {
            var temp = RelationViewModel;
            if (e.Key == Key.Enter)
            {
                await SendTextMessageAsync();
            }

            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var dataObject = Clipboard.GetDataObject();
                if (dataObject.GetDataPresent(DataFormats.FileDrop)) //文件
                {
                    var files = Clipboard.GetFileDropList();
                    var result = HandyControl.Controls.MessageBox
                        .Ask($"确认将[{Path.GetFileName(files[0])}]等[{files.Count}]个文件发送给\"{temp.Name}\"吗?", "发送文件提示");

                    if (result == MessageBoxResult.OK)
                    {
                        foreach (var file in files)
                        {
                            var fileinfo = new FileInfo(file);
                            if (fileinfo.Length > 50 * 1024 * 1024)
                            {
                                _growlHelper.Warning($"{fileinfo.Name}超过50M,无法发送");
                                continue;
                            }

                            await SendFileAsync(fileinfo, temp);
                        }
                    }

                    e.Handled = true;
                }
                else if (dataObject.GetDataPresent(DataFormats.Bitmap)) //图片
                {
                    var path = _fileHelper.SaveImageToFile(Clipboard.GetImage());
                    await SendImageFileAsync(new FileInfo(path), temp);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 完善聊天记录
        /// </summary>
        /// <param name="friendId">服务器拉取的聊天记录</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<IEnumerable<MessageViewModel>> ConvertChatRecordToViewModel(IEnumerable<ChatRecordResponseDto> resp, bool desc = false)
        {
            if (resp == null)
                return null;

            var localImageRecords = resp.Where(x => x.MessageRecordType == MessageRecordType.Image
                && x.ImageType == ImageType.Local && !x.Withdrawed);
            if (localImageRecords != null && localImageRecords.Count() > 0)
            {
                //查找本地数据库图片位置
                //只有图片文件需要即时下载
                var localImageRecordsIds = localImageRecords.Select(x => x.MessageId);
                var localImageRecordsInDb = await SqlMapper.QueryAsync<ChatRecord>(_clientSqliteContext.dbConnection,
                                       "SELECT * FROM ChatRecord WHERE MessageId IN @Ids", new { Ids = localImageRecordsIds });
                foreach (var record in localImageRecords)
                {
                    var imageMessage = localImageRecordsInDb.FirstOrDefault(x => x.MessageId == record.MessageId);
                    if (imageMessage != null && File.Exists(imageMessage.ResourceLocalLocation))
                    {
                        record.Message = imageMessage.ResourceLocalLocation;
                    }
                    else
                    {
                        //向远程服务器发起请求
                        var bytes = await _contextAPIService.DownloadFileAsync(record.MessageId, null);
                        if (bytes == null)
                        {
                            //TODO给一个未知图片的信息或者过期的默认图片
                            continue;
                        }

                        var path = await _fileHelper.AutoSaveFileByBytesAsync(bytes, record.FileName, MessageType.Image);
                        if (string.IsNullOrEmpty(path))
                        {
                            //TODO给一个未知图片的信息或者过期的默认图片
                            continue;
                        }
                        else
                        {
                            if (imageMessage == null)
                            {
                                await _saveChatRecordService.WriteRecordAsync(new ChatRecord
                                {
                                    From = record.From,
                                    To = record.To,
                                    MessageId = record.MessageId,
                                    Message = record.FileName,
                                    ResourceLocalLocation = path,
                                    MessageRecordType = MessageRecordType.Image,
                                    ImageType = ImageType.Local,
                                    SendTime = record.SendTime,
                                    FileId = record.FileId
                                });
                            }
                            else
                            {
                                await SqlMapper.ExecuteAsync(_clientSqliteContext.dbConnection,
                                           "UPDATE ChatRecord SET ResourceLocalLocation = @Location WHERE MessageId = @Id",
                                           new { Id = record.MessageId, Location = path });
                            }

                            record.Message = path;
                        }
                    }
                }
            }

            var netImageRecords = resp.Where(x => x.MessageRecordType == MessageRecordType.Image
                && x.ImageType == ImageType.Network && !x.Withdrawed);
            if (netImageRecords != null && netImageRecords.Count() > 0)
            {
                foreach (var record in netImageRecords)
                {
                    record.Bytes = await _contextAPIService.DownloadNetworkImageAsync(record.Message);
                }
            }

            List<MessageViewModel> messages = new List<MessageViewModel>();
            foreach (var record in desc ? resp.OrderByDescending(x => x.SendTime) : resp.OrderBy(x => x.SendTime))
            {
                try
                {
                    if (record.Withdrawed)
                    {
                        MessageViewModel temp = new MessageViewModel(MessageType.Text);
                        temp.MessageId = record.MessageId;
                        temp.Withdrawed = true;
                        temp.From = record.From;
                        temp.To = record.To;
                        temp.SendTime = record.SendTime;
                        messages.Add(temp);
                        continue;
                    }
                    MessageViewModel messageViewModel = null;
                    if (record.MessageRecordType == MessageRecordType.Text)
                    {
                        messageViewModel = new TextMessageViewModel();
                        messageViewModel.FromDto(record);
                    }

                    if (record.MessageRecordType == MessageRecordType.Image)
                    {
                        messageViewModel = new ImageMessageViewModel();
                        messageViewModel.FromDto(record);
                    }

                    if (record.MessageRecordType == MessageRecordType.File)
                    {
                        messageViewModel = new FileMessageViewModel();
                        messageViewModel.FromDto(record);
                    }

                    if (record.MessageRecordType == MessageRecordType.VoiceChat)
                    {
                        messageViewModel = new VoiceChatViewModel();
                        messageViewModel.FromDto(record);
                    }
                    messages.Add(messageViewModel);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            return messages;
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <returns></returns>
        private async Task SendTextMessageAsync()
        {
            var tempFriend = RelationViewModel;
            if (string.IsNullOrEmpty(TextMessage))
                return;

            if (TextMessage.Length > 500)
            {
                _growlHelper.Warning("文本长度不可超过500");
                return;
            }

            var viewModel = new TextMessageViewModel()
            {
                Text = textMessage,
                SendTime = DateTime.Now,
                From = App.UserProfile.UserId,
                To = tempFriend.Id,
                IsGroup = tempFriend.IsGroup,
                Sending = true,
            };

            await SendTextMessageByVmAsync(viewModel, tempFriend);
        }

        private async Task SendTextMessageByVmAsync(TextMessageViewModel viewModel, RelationViewModel friend)
        {
            friend.AddMessage(viewModel);
            var body = new TextMessage();
            viewModel.ToMessage(body);
            var packet = new Packet<TextMessage>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.Text,
                Body = body
            };

            var sendResult = await _tcpSuperSocketClient.SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                viewModel.Sending = false;
                viewModel.SendSuccess = false;
                _growlHelper.Warning("发送消息失败，请稍后再试");
            }

            TextMessage = string.Empty;
        }

        /// <summary>
        /// 发送表情消息
        /// </summary>
        /// <param name="routedEventArgs">选择的表情</param>
        /// <returns></returns>
        private async Task SendEmojiAsync(SelectEmojiClickRoutedEventArgs routedEventArgs)
        {
            IsEmojiOpen = false;
            var tempFriend = RelationViewModel;
            var viewModel = new ImageMessageViewModel()
            {
                ImageType = ImageType.Network,
                SendTime = DateTime.Now,
                From = App.UserProfile.UserId,
                To = tempFriend.Id,
                IsGroup = tempFriend.IsGroup,
                Sending = true,
                RemoteUrl = routedEventArgs.Emoji.Url
            };

            var bytes = await _contextAPIService.DownloadNetworkImageAsync(viewModel.RemoteUrl);
            viewModel.Source = _fileHelper.ByteArrayToBitmapImage(bytes);
            await SendEmojiByVmAsync(viewModel, tempFriend);
        }

        private async Task SendEmojiByVmAsync(ImageMessageViewModel viewModel, RelationViewModel friend)
        {
            friend.AddMessage(viewModel);
            var body = new ImageMessage();
            viewModel.ToMessage(body);
            var packet = new Packet<ImageMessage>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.Image,
                Body = body
            };

            var sendResult = await _tcpSuperSocketClient.SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                viewModel.Sending = false;
                viewModel.SendSuccess = false;
                _growlHelper.Warning("发送消息失败，请稍后再试");
            }
        }

        private void OpenEmoji()
        {
            IsEmojiOpen = true;
        }

        private async Task ChooseFileSendAsync()
        {
            var tempViewModel = RelationViewModel;
            if (tempViewModel != null)
            {
                var openFileDialog = new OpenFileDialog();
                var result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    var path = openFileDialog.FileName;
                    FileInfo fileInfo = new FileInfo(path);
                    if (fileInfo.Length > 50 * 1024 * 1024)
                    {
                        _growlHelper.Warning("文件大小不可以超过50M");
                        return;
                    }
                    try
                    {
                        if (IsRealImage(path))
                        {
                            await SendImageFileAsync(fileInfo, tempViewModel);
                        }
                        else //其他文件
                        {
                            await SendFileAsync(fileInfo, tempViewModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        _growlHelper.Warning($"{ex.Message}");
                        _logger.LogError(ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 上传文件到服务器
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="friendId">发送好友的id</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<long> UploadFileAsync(FileInfo fileInfo, long relation, bool isGroup)
        {
            var data = await File.ReadAllBytesAsync(fileInfo.FullName);
            string code = data.ToSHA256();
            var resp = await _contextAPIService.GetAsync<CheckFileExistResponseDto>(string.Format(Const.FILE_EXIST, code));
            if (resp == null) throw new Exception("服务器异常");

            if (!resp.Exist)//文件存在，直接发送
            {
                var fileResp = await _contextAPIService.UploadFileAsync<UploadFileResponseDto>
                    (fileInfo.Name, data, code, isGroup, relation, UploadProgressCallback, false);

                if (fileResp == null) throw new Exception("发送文件异常，请稍后再试");
                resp.FileId = fileResp.FileId;
            }

            return resp.FileId;
        }

        /// <summary>
        /// 发送图片文件给好友
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="friend">好友</param>
        /// <returns></returns>
        private async Task SendImageFileAsync(FileInfo fileInfo, RelationViewModel friend)
        {
            var viewModel = new ImageMessageViewModel()
            {
                ImageType = ImageType.Local,
                SendTime = DateTime.Now,
                From = App.UserProfile.UserId,
                Location = fileInfo.FullName,
                To = friend.Id,
                IsGroup = friend.IsGroup,
                Sending = true
            };

            viewModel.Source = _fileHelper.ByteArrayToBitmapImage(await File.ReadAllBytesAsync(fileInfo.FullName));
            await SendImageByVmAsync(viewModel, friend, fileInfo);
        }

        private async Task SendImageByVmAsync(ImageMessageViewModel viewModel, RelationViewModel friend, FileInfo fileInfo)
        {
            friend.AddMessage(viewModel);
            if (viewModel.FileId == default)
            {
                viewModel.FileId = await UploadFileAsync(fileInfo, friend.Id, friend.IsGroup);
            }
            var body = new ImageMessage();
            viewModel.ToMessage(body);
            var packet = new Packet<ImageMessage>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.Image,
                Body = body
            };

            var sendResult = await _tcpSuperSocketClient.SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                viewModel.Sending = false;
                viewModel.SendSuccess = false;
                _growlHelper.Warning("发送消息失败，请稍后再试");
            }
            else
            {
                _messageFileLocationMapper.AddOrUpdate(packet.Body.MessageId, viewModel.Location);
            }
        }

        /// <summary>
        /// 发送文件给好友
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="friend">好友</param>
        /// <returns></returns>
        private async Task SendFileAsync(FileInfo fileInfo, RelationViewModel friend)
        {
            var viewModel = new FileMessageViewModel(fileInfo.Name)
            {
                FileSize = fileInfo.Length,
                SendTime = DateTime.Now,
                From = App.UserProfile.UserId,
                Location = fileInfo.FullName,
                To = friend.Id,
                IsGroup = friend.IsGroup,
                Sending = true
            };

            await SendFileByVmAsync(viewModel, friend, fileInfo);
        }

        private async Task SendFileByVmAsync(FileMessageViewModel viewModel, RelationViewModel relation, FileInfo fileInfo)
        {
            relation.AddMessage(viewModel);
            if (viewModel.FileId == default)
            {
                var fileId = await UploadFileAsync(fileInfo, viewModel.To, relation.IsGroup);
                viewModel.FileId = fileId;
            }
            var body = new FileMessage();
            viewModel.ToMessage(body);
            var packet = new Packet<FileMessage>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.File,
                Body = body
            };

            var sendResult = await _tcpSuperSocketClient.SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                viewModel.Sending = false;
                viewModel.SendSuccess = false;
                _growlHelper.Warning("发送消息失败，请稍后再试");
            }
            else
            {
                _messageFileLocationMapper.AddOrUpdate(packet.Body.MessageId, viewModel.Location);
            }
        }

        private void UploadProgressCallback(object obj, HttpProgressEventArgs args)
        {
            Console.WriteLine(args.ProgressPercentage);
        }

        /// <summary>
        /// 重发发送失败的消息
        /// </summary>
        /// <param name="viewModel">重新发送的消息模型</param>
        /// <returns></returns>
        private async Task ResendMessageAsync(MessageViewModel viewModel)
        {
            var friend = RelationViewModels.FirstOrDefault(x => x.Id == viewModel.To);
            if (friend == null)
            {
                return;
            }
            viewModel.SendTime = DateTime.Now;
            if (viewModel is TextMessageViewModel)
            {
                await SendTextMessageByVmAsync(viewModel as TextMessageViewModel, friend);
            }
            else if (viewModel is ImageMessageViewModel)
            {
                var image = viewModel as ImageMessageViewModel;
                if (image.ImageType == ImageType.Local)
                {
                    await SendImageByVmAsync(image, friend, image.FileId == default ? new FileInfo(image.Location) : null);
                }
                else
                {
                    await SendEmojiByVmAsync(image, friend);
                }
            }
            else if (viewModel is FileMessageViewModel)
            {
                var file = viewModel as FileMessageViewModel;
                if (file.FileId == default && !File.Exists(file.Location))
                    return;

                await SendFileByVmAsync(file, friend, file.FileId == default ? new FileInfo(file.Location) : null);
            }
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="viewModel">重新发送的消息模型</param>
        /// <returns></returns>
        private async Task WithdrawMessageAsync(MessageViewModel viewModel)
        {
            if (!viewModel.FromSelf)
                return;

            var packet = new Packet<WithdrawMessage>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.Withdraw,
                Body = new WithdrawMessage()
                {
                    To = viewModel.To,
                    IsGroup = viewModel.IsGroup,
                    WithdrawMessageId = viewModel.MessageId
                }
            };

            var sendResult = await _tcpSuperSocketClient.SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                viewModel.Sending = false;
                viewModel.SendSuccess = false;
                _growlHelper.Warning("撤回消息失败，请稍后再试");
            }
        }

        private void WithdrawMessage(WithdrawMessageResponse response, RelationViewModel relation)
        {
            var message = relation?.Messages
                .FirstOrDefault(x => x.MessageId == response.WithdrawMessageId);
            if (message != null)
            {
                message.Withdrawed = true;
            }
        }

        #region 收到新消息
        /// <summary>
        /// 收到新的文本信息
        /// </summary>
        /// <param name="textMessageResponse">文本信息响应</param>
        /// <param name="relation">关联主体</param>
        /// <returns></returns>
        private async Task ReceiveTextMessageAsync(TextMessageResponse textMessageResponse, RelationViewModel relation)
        {
            if (textMessageResponse.Result)
            {
                await _saveChatRecordService.WriteRecordAsync(new ChatRecord
                {
                    From = textMessageResponse.From,
                    To = textMessageResponse.To,
                    MessageId = textMessageResponse.MessageId,
                    Message = textMessageResponse.Text,
                    MessageRecordType = MessageRecordType.Text,
                    SendTime = textMessageResponse.SendTime
                });
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                relation.AddMessage(new TextMessageViewModel()
                {
                    Text = textMessageResponse.Text,
                    IsGroup = textMessageResponse.IsGroup,
                    SendTime = textMessageResponse.SendTime,
                    MessageId = textMessageResponse.MessageId,
                    From = textMessageResponse.From,
                    FromName = textMessageResponse.FromName,
                    To = textMessageResponse.To,
                    Sending = false,
                    SendSuccess = textMessageResponse.Result
                });
            });
        }

        /// <summary>
        /// 收到新的图片信息
        /// </summary>
        /// <param name="textMessageResponse">文本信息响应</param>
        /// <param name="relation">关联主体</param>
        /// <returns></returns>
        private async Task ReceiveImageMessageAsync(ImageMessageResponse imageMessageResponse, RelationViewModel relation)
        {
            var image = new ImageMessageViewModel()
            {
                ImageType = imageMessageResponse.ImageType,
                SendTime = imageMessageResponse.SendTime,
                MessageId = imageMessageResponse.MessageId,
                IsGroup = imageMessageResponse.IsGroup,
                From = imageMessageResponse.From,
                FromName = imageMessageResponse.FromName,
                To = imageMessageResponse.To,
                Sending = false,
                SendSuccess = imageMessageResponse.Result,
                RemoteUrl = imageMessageResponse.RemoteUrl
            };

            if (imageMessageResponse.Result)
            {
                string imageLocation = _messageFileLocationMapper.Pop(imageMessageResponse.MessageId);
                if (string.IsNullOrEmpty(imageLocation) || !File.Exists(imageLocation))
                {
                    var bytes = image.ImageType == ImageType.Local ?
                            await _contextAPIService.DownloadFileAsync(imageMessageResponse.MessageId, null)
                            :
                            await _contextAPIService.DownloadNetworkImageAsync(imageMessageResponse.RemoteUrl, null);

                    if (bytes == null)
                    {
                        _growlHelper.Warning("接收图片出现异常");
                        return;
                    }

                    if (image.ImageType == ImageType.Local)
                    {
                        imageLocation = await _fileHelper.AutoSaveFileByBytesAsync(bytes, imageMessageResponse.FileName, MessageType.Image);
                        if (string.IsNullOrEmpty(imageLocation))
                        {
                            _growlHelper.Warning("接收图片出现异常");
                            return;
                        }
                    }

                    image.Location = imageLocation;
                    image.Source = _fileHelper.ByteArrayToBitmapImage(bytes);
                }
                else
                {
                    image.Location = imageLocation;
                    image.Source = _fileHelper.ByteArrayToBitmapImage(await File.ReadAllBytesAsync(imageLocation));
                }

                await _saveChatRecordService.WriteRecordAsync(new ChatRecord
                {
                    From = imageMessageResponse.From,
                    To = imageMessageResponse.To,
                    MessageId = imageMessageResponse.MessageId,
                    Message = image.ImageType == ImageType.Network ? "表情包" : imageMessageResponse.FileName,
                    ResourceLocalLocation = image.ImageType == ImageType.Network ? imageMessageResponse.RemoteUrl : imageLocation,
                    MessageRecordType = MessageRecordType.Image,
                    ImageType = image.ImageType,
                    SendTime = imageMessageResponse.SendTime,
                    FileId = imageMessageResponse.FileId == 0 ? null : imageMessageResponse.FileId,
                });
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                relation.AddMessage(image);
            });
        }

        /// <summary>
        /// 收到新的文件信息
        /// </summary>
        /// <param name="textMessageResponse">文本信息响应</param>
        /// <param name="relation">关联主体</param>
        /// <returns></returns>
        private async Task ReceiveFileMessageAsync(FileMessageResponse fileMessageResponse, RelationViewModel relation)
        {
            var path = _messageFileLocationMapper.Pop(fileMessageResponse.MessageId);
            if (fileMessageResponse.Result)
            {
                await _saveChatRecordService.WriteRecordAsync(new ChatRecord
                {
                    From = fileMessageResponse.From,
                    To = fileMessageResponse.To,
                    MessageId = fileMessageResponse.MessageId,
                    Message = fileMessageResponse.FileName,
                    MessageRecordType = MessageRecordType.File,
                    ResourceLocalLocation = path,
                    SendTime = fileMessageResponse.SendTime,
                    FileId = fileMessageResponse.FileId == 0 ? null : fileMessageResponse.FileId,
                    ResourceSize = fileMessageResponse.Size
                });
            }

            var filevm = new FileMessageViewModel(fileMessageResponse.FileName)
            {
                FileSize = fileMessageResponse.Size,
                SendTime = fileMessageResponse.SendTime,
                MessageId = fileMessageResponse.MessageId,
                From = fileMessageResponse.From,
                FromName = fileMessageResponse.FromName,
                IsGroup = fileMessageResponse.IsGroup,
                To = fileMessageResponse.To,
                Sending = false,
                SendSuccess = fileMessageResponse.Result
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                relation.AddMessage(filevm);
            });
        }

        /// <summary>
        /// 收到新的语音记录
        /// </summary>
        /// <param name="textMessageResponse">文本信息响应</param>
        /// <param name="relation">关联主体</param>
        /// <returns></returns>
        private async Task ReceiveVoiceChatMessageAsync(VoiceChatMessageResponse messageResponse, RelationViewModel relation)
        {
            //TODO记录数据库
            var chatvm = new VoiceChatViewModel()
            {
                SendTime = messageResponse.SendTime,
                From = messageResponse.From,
                FromName = messageResponse.FromName,
                To = messageResponse.To,
                VoiceChatStatus = VoiceChatStatus.NotAccept
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                relation.AddMessage(chatvm);
            });
        }

        //获取消息中的聊天主体
        private RelationViewModel GetRelation(long from, long to, bool isGroup)
        {
            if (isGroup)
            {
                return RelationViewModels.FirstOrDefault(x => x.IsGroup && x.Id == to);
            }
            else
            {
                return from == App.UserProfile.UserId ?
                    RelationViewModels.FirstOrDefault(x => x.Id == to) :
                    RelationViewModels.FirstOrDefault(x => x.Id == from);
            }
        }
        #endregion

        private double _lastVehicleOffset = 0;
        private double _currentVehicleOffset = 0;
        private bool _firstTop = false;
        private async void ChatScrollChange(ScrollChangedEventArgs eventArgs)
        {
            var scrollViewer = eventArgs.OriginalSource as ScrollViewer;
            _lastVehicleOffset = _currentVehicleOffset;
            _currentVehicleOffset = eventArgs.VerticalOffset;

            if (_lastVehicleOffset != 0 && _currentVehicleOffset == 0 && eventArgs.VerticalChange != 0)
            {
                if (!_firstTop)
                {
                    _firstTop = true;
                    return;
                }
                _firstTop = false;
                //同步请求聊天记录
                var temp = RelationViewModel;
                if (temp.FetchFirstRecordTime == null)
                {
                    _growlHelper.Info("没有更多的聊天记录");
                    return;
                }

                var records = _contextAPIService.GetAsync<IEnumerable<ChatRecordResponseDto>>(string.Format(Const.FETCH_RELATION_CHATRECORDS,
                    temp.Id, temp.IsGroup, temp.FetchFirstRecordTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"))).GetAwaiter().GetResult();

                if (records != null && records.Count() != 0)
                {
                    var messages = await ConvertChatRecordToViewModel(records, true);
                    var height = scrollViewer.ExtentHeight;
                    temp.AddMessages(messages, true);
                    var _ = Task.Run(() =>
                    {
                        Task.Delay(500);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var newHeight = scrollViewer.ExtentHeight;
                            scrollViewer.ScrollToVerticalOffset(newHeight - height);
                        });
                    });
                }
                else if (records != null && records.Count() == 0)
                {
                    _growlHelper.Info("没有更多的聊天记录");
                }
            }
        }

        private void InitScrollFlags()
        {
            _lastVehicleOffset = 0;
            _currentVehicleOffset = 0;
            _firstTop = false;
        }

        public bool IsRealImage(string path)
        {
            try
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

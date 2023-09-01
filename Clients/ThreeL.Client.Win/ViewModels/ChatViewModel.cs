using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Handlers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
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
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SelectFriendCommandAsync { get; set; }
        public AsyncRelayCommand<SelectEmojiClickRoutedEventArgs> AddEmojiCommandAsync { get; set; }
        public AsyncRelayCommand SendMessageCommandAsync { get; set; }
        public AsyncRelayCommand ChooseFileSendCommandAsync { get; set; }
        public AsyncRelayCommand<KeyEventArgs> SendTextboxKeyDownCommandAsync { get; set; }
        public RelayCommand GotoSettingsPageCommand { get; set; }
        public AsyncRelayCommand DisplayDetailCommand { get; set; }

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
            set => SetProperty(ref relationViewModel, value);
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

        private bool _isLoaded = false;
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
                             MessageFileLocationMapper messageFileLocationMapper)
        {
            _contextAPIService = contextAPIService;
            _clientSqliteContext = clientSqliteContext;
            _saveChatRecordService = saveChatRecordService;
            _growlHelper = growlHelper;
            _packetWaiter = packetWaiter;
            _fileHelper = fileHelper;
            _udpSuperSocketClient = udpSuperSocketClient;
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
            _sequenceIncrementer = sequenceIncrementer;
            _tcpSuperSocketClient = tcpSuperSocketClient;
            _messageFileLocationMapper = messageFileLocationMapper;
            RelationViewModels = new ObservableCollection<RelationViewModel>();

            WeakReferenceMessenger.Default.Register<ChatViewModel, FromToMessageResponse, string>(this, "message-send-result",
                (x, y) =>
            {
                var message = RelationViewModels.FirstOrDefault(x => x.Id == y.To)?
                .Messages.FirstOrDefault(x => x.MessageId == y.MessageId);

                if (message != null)
                {
                    message.SendSuccess = y.Result;
                    if (!message.SendSuccess && y.From == App.UserProfile.UserId)
                    {
                        _growlHelper.Warning($"消息发送失败:[{y.Message}]");
                    }
                }
            });

            WeakReferenceMessenger.Default.Register<ChatViewModel, FromToMessageResponse, string>(this, "message-send-finished",
                (x, y) =>
                {
                    var message = RelationViewModels.FirstOrDefault(x => x.Id == y.To)?
                    .Messages.FirstOrDefault(x => x.MessageId == y.MessageId);

                    if (message != null)
                    {
                        message.Sending = false;
                    }
                });

            WeakReferenceMessenger.Default.Register<ChatViewModel, WithdrawMessageResponse, string>(this, "message-withdraw-result",
                (x, y) =>
                {
                    WithdrawMessage(y);
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

            WeakReferenceMessenger.Default.Register<ChatViewModel, GroupCreationResponseDto, string>(this, "message-newgroup",
              async (x, y) =>
              {
                  await HandleNewGroupAsync(y);
              });
        }

        private async Task LoadAsync()
        {
            if (!_isLoaded)
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
                    
                    var relations = await _contextAPIService.GetAsync<IEnumerable<RelationDto>>(string.Format(Const.FETCH_RELATIONS, INITIALIZE_TIME.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                    if (relations != null)
                    {

                        foreach (var rel in relations)
                        {
                            var fvm = new RelationViewModel
                            {
                                Id = rel.Id,
                                Name = rel.Name,
                                AvatarId = rel.Avatar,
                                Remark = rel.Remark,
                                IsGroup = rel.IsGroup,
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

                        ////加载好友列表
                        //FriendViewModels = new ObservableCollection<FriendViewModel>(friends)
                        //{
                        //    new FriendViewModel()
                        //    {
                        //        Id = App.UserProfile.UserId,
                        //        Remark = "本人",
                        //        AvatarId = App.UserProfile.AvatarId,
                        //        UserName = App.UserProfile.UserName,
                        //        IsGroup = false
                        //    }
                        //};
                        RelationViewModel = RelationViewModels.FirstOrDefault();
                        _isLoaded = true;

                        //加载申请好友记录
                        await FetchUserUnProcessApplysAsync();
                    }
                }
                catch (Exception ex)
                {
                    _growlHelper.Warning(ex.Message);
                }
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

        private async Task HandleNewGroupAsync(GroupCreationResponseDto group) 
        {
            RelationViewModels.Insert(0, new RelationViewModel()
            {
                IsGroup = true,
                Id = group.Id,
                Name = group.Name,
                AvatarId = group.Avatar,
            });

            //load群聊聊天记录
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
            WeakReferenceMessenger.Default.Send<string, string>("setting", "switch-page");
        }

        private void GotoApplyPage()
        {
            WeakReferenceMessenger.Default.Send<string, string>("apply", "switch-page");
        }

        private async Task DisplayDetailAsync() 
        {
            if (RelationViewModel == null)
                return;

            if (RelationViewModel.IsGroup)
            {
                var group = await _contextAPIService.GetAsync<GroupRoughlyDto>(string.Format(Const.GROUP_DETAIL, RelationViewModel.Id));
                if (group == null)
                    return;

                var window = App.ServiceProvider.GetRequiredService<GroupDetailWindow>();
                window.DataContext = new GroupDetailWindowViewModel().FromDto(group);
                window.Owner = App.ServiceProvider.GetRequiredService<MainWindow>();
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();
            }
            else 
            {
                var user =  await _contextAPIService.GetAsync<UserRoughlyDto>(string.Format(Const.USER, RelationViewModel.Id));
                if (user == null)
                    return;

                DetailWindowViewModel = new UserDetailWindowViewModel().FromDto(user);
                IsUserDetailOpen = true;
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
                        .Ask($"确认将[{Path.GetFileName(files[0])}]等[{files.Count}]个文件发送给\"{temp.Name}\"吗?");

                    if (result == MessageBoxResult.OK)
                    {

                        foreach (var file in files)
                        {
                            await SendFileAsync(new FileInfo(file), temp);
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
        private async Task<IEnumerable<MessageViewModel>> ConvertChatRecordToViewModel(IEnumerable<ChatRecordResponseDto> resp)
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
            foreach (var record in resp.OrderBy(x => x.SendTime))
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

                messages.Add(messageViewModel);
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
        private async Task<long> UploadFileAsync(FileInfo fileInfo, long friendId)
        {
            var data = await File.ReadAllBytesAsync(fileInfo.FullName);
            string code = data.ToSHA256();
            var resp = await _contextAPIService.GetAsync<CheckFileExistResponseDto>(string.Format(Const.FILE_EXIST, code));
            if (resp == null) throw new Exception("服务器异常");

            if (!resp.Exist)//文件存在，直接发送
            {
                var fileResp = await _contextAPIService.UploadFileAsync<UploadFileResponseDto>
                    (fileInfo.Name, data, code, friendId, UploadProgressCallback, false);

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
                viewModel.FileId = await UploadFileAsync(fileInfo, friend.Id);
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

        private async Task SendFileByVmAsync(FileMessageViewModel viewModel, RelationViewModel friend, FileInfo fileInfo)
        {
            friend.AddMessage(viewModel);
            if (viewModel.FileId == default)
            {
                var fileId = await UploadFileAsync(fileInfo, viewModel.To);
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

        private void WithdrawMessage(WithdrawMessageResponse response)
        {
            var friend = App.UserProfile.UserId == response.From ? response.To : response.From;
            var message = RelationViewModels.FirstOrDefault(x => x.Id == friend)?.Messages
                .FirstOrDefault(x => x.MessageId == response.WithdrawMessageId);
            if (message != null)
            {
                message.Withdrawed = true;
                message.WithdrawMessage = App.UserProfile.UserId == message.From ? "你撤回了一条消息" : "对方撤回了一条消息";
            }
        }

        public bool IsRealImage(string path)
        {
            try
            {
                Image img = Image.FromFile(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

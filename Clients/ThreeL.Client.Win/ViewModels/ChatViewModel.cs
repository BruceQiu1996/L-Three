using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dapper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Handlers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Database;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Entities;
using ThreeL.Client.Shared.Entities.Metadata;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.Helpers;
using ThreeL.Client.Win.MyControls;
using ThreeL.Client.Win.ViewModels.Messages;
using ThreeL.Infra.Core.Cryptography;
using ThreeL.Shared.SuperSocket.Cache;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        public RelayCommand OpenEmojiCommand { get; set; }
        public AsyncRelayCommand LoadCommandAsync { get; set; }
        public AsyncRelayCommand SelectFriendCommandAsync { get; set; }
        public AsyncRelayCommand<SelectEmojiClickRoutedEventArgs> AddEmojiCommandAsync { get; set; }
        public AsyncRelayCommand SendMessageCommandAsync { get; set; }
        public AsyncRelayCommand ChooseFileSendCommandAsync { get; set; }

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

        private UserProfile userProfile;
        public UserProfile UserProfile
        {
            get { return userProfile; }
            set => SetProperty(ref userProfile, value);
        }

        private readonly ContextAPIService _contextAPIService;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly GrowlHelper _growlHelper;
        private readonly FileHelper _fileHelper;
        private readonly UdpSuperSocketClient _udpSuperSocketClient;
        private readonly PacketWaiter _packetWaiter;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly TcpSuperSocketClient _tcpSuperSocketClient;
        private readonly IPEndPoint _iPEndPoint;

        public ChatViewModel(ContextAPIService contextAPIService,
                             ClientSqliteContext clientSqliteContext,
                             GrowlHelper growlHelper,
                             FileHelper fileHelper,
                             PacketWaiter packetWaiter,
                             IPEndPoint iPEndPoint,
                             UdpSuperSocketClient udpSuperSocketClient,
                             SequenceIncrementer sequenceIncrementer,
                             TcpSuperSocketClient tcpSuperSocketClient)
        {
            _contextAPIService = contextAPIService;
            _clientSqliteContext = clientSqliteContext;
            _growlHelper = growlHelper;
            _packetWaiter = packetWaiter;
            _iPEndPoint = iPEndPoint;
            _fileHelper = fileHelper;
            _udpSuperSocketClient = udpSuperSocketClient;
            LoadCommandAsync = new AsyncRelayCommand(LoadAsync);
            SelectFriendCommandAsync = new AsyncRelayCommand(SelectFriendAsync);
            SendMessageCommandAsync = new AsyncRelayCommand(SendMessageAsync);
            AddEmojiCommandAsync = new AsyncRelayCommand<SelectEmojiClickRoutedEventArgs>(AddEmojiAsync);
            OpenEmojiCommand = new RelayCommand(OpenEmoji);
            ChooseFileSendCommandAsync = new AsyncRelayCommand(ChooseFileSendAsync);
            _sequenceIncrementer = sequenceIncrementer;
            _tcpSuperSocketClient = tcpSuperSocketClient;
        }

        private async Task LoadAsync()
        {
            UserProfile = App.UserProfile;
            //TODO删除代码
            try
            {
                //获取好友列表
                var resp = await _contextAPIService.GetAsync<IEnumerable<FriendDto>>(Const.FETCH_FRIENDS);
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
                    FriendViewModels = new ObservableCollection<FriendViewModel>(friends)
                    {
                        new FriendViewModel()
                        {
                            Id = App.UserProfile.UserId,
                            Remark = "我自己",
                            UserName = App.UserProfile.UserName,
                        }
                    };

                    FriendViewModel = FriendViewModels.FirstOrDefault();
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
            var tempFriend = FriendViewModel;
            if (!tempFriend.LoadedChatRecord)
            {
                //加载历史100条聊天记录 //TODO与服务器聊天记录做比较
                var redords =
                    await SqlMapper.QueryAsync<ChatRecord>(_clientSqliteContext.dbConnection,
                        "SELECT * FROM ChatRecord WHERE (([FROM] = @Id AND [TO] = @Cid) OR ([TO] = @Id and [FROM] = @Cid)) AND rowid IN (SELECT rowid FROM ChatRecord ORDER BY SendTime Desc LIMIT 100)",
                        new { tempFriend.Id, Cid = App.UserProfile.UserId });

                if (redords != null && redords.Count() > 0)
                {
                    foreach (var record in redords.OrderBy(x => x.SendTime))
                    {
                        MessageViewModel messageViewModel = null;
                        if (record.MessageRecordType == MessageRecordType.Text)
                        {
                            messageViewModel = new TextMessageViewModel();
                            messageViewModel.FromEntity(record);
                        }

                        if (record.MessageRecordType == MessageRecordType.Image)
                        {
                            messageViewModel = new ImageMessageViewModel();
                            messageViewModel.FromEntity(record);
                        }

                        tempFriend.AddMessage(messageViewModel);
                    }
                }
                tempFriend.LoadedChatRecord = true;
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

            await _tcpSuperSocketClient.SendBytes(packet.Serialize());
            TextMessage = string.Empty;
            //foreach (var endpoint in FriendViewModel.Hosts)
            //{
            //    await _udpSuperSocketClient.SendBytes(IPEndPoint.Parse(endpoint), packet.Serialize());
            //}
        }

        private async Task AddEmojiAsync(SelectEmojiClickRoutedEventArgs routedEventArgs)
        {
            IsEmojiOpen = false;
            if (FriendViewModel != null)
            {
                var packet = new Packet<ImageMessage>()
                {
                    Sequence = _sequenceIncrementer.GetNextSequence(),
                    MessageType = MessageType.Image,
                    Body = new ImageMessage
                    {
                        SendTime = DateTime.Now,
                        From = App.UserProfile.UserId,
                        To = FriendViewModel.Id,
                        ImageType = (byte)routedEventArgs.Emoji.ImageType,
                        RemoteUrl = routedEventArgs.Emoji.Url //如果是bitmap需要转byte[]
                    }
                };

                await _tcpSuperSocketClient.SendBytes(packet.Serialize());
            }
        }

        private void OpenEmoji()
        {
            IsEmojiOpen = true;
        }

        private async Task ChooseFileSendAsync()
        {
            var tempViewModel = FriendViewModel;
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
                        var data = await File.ReadAllBytesAsync(fileInfo.FullName);
                        string code = data.ToSHA256();
                        var resp = await _contextAPIService
                            .GetAsync<CheckFileExistResponseDto>(string.Format(Const.FILE_EXIST, code));

                        if (resp == null) throw new Exception("服务器异常");

                        if (!resp.Exist)//文件存在，直接发送
                        {
                            var fileResp = await _contextAPIService.UploadFileAsync<UploadFileResponseDto>
                                (fileInfo.Name, data, code, tempViewModel.Id, UploadProgressCallback, false);

                            if (fileResp == null) throw new Exception("发送文件异常，请稍后再试");
                            resp.FileId = fileResp.FileId;
                        }

                        if (IsRealImage(path))
                        {
                            var packet = new Packet<ImageMessage>()
                            {
                                Sequence = _sequenceIncrementer.GetNextSequence(),
                                MessageType = MessageType.Image,
                                Body = new ImageMessage
                                {
                                    SendTime = DateTime.Now,
                                    From = App.UserProfile.UserId,
                                    To = FriendViewModel.Id,
                                    ImageType = (byte)ImageType.Local,
                                    FileId = resp.FileId,
                                    FileName = fileInfo.Name
                                }
                            };

                            await _tcpSuperSocketClient.SendBytes(packet.Serialize());
                        }
                        else //其他文件
                        {

                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                    }
                }
            }
        }

        private void UploadProgressCallback(object obj, HttpProgressEventArgs args)
        {
            Console.WriteLine(args.ProgressPercentage);
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

﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Handlers;
using System.Threading.Tasks;
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
        private readonly SaveChatRecordService _saveChatRecordService;
        private readonly MessageFileLocationMapper _messageFileLocationMapper;

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
            SelectFriendCommandAsync = new AsyncRelayCommand(SelectFriendAsync);
            SendMessageCommandAsync = new AsyncRelayCommand(SendTextMessageAsync);
            AddEmojiCommandAsync = new AsyncRelayCommand<SelectEmojiClickRoutedEventArgs>(SendEmojiAsync);
            OpenEmojiCommand = new RelayCommand(OpenEmoji);
            ChooseFileSendCommandAsync = new AsyncRelayCommand(ChooseFileSendAsync);
            _sequenceIncrementer = sequenceIncrementer;
            _tcpSuperSocketClient = tcpSuperSocketClient;
            _messageFileLocationMapper = messageFileLocationMapper;

            WeakReferenceMessenger.Default.Register<ChatViewModel, dynamic, string>(this, "message-send-faild", (x, y) =>
            {
                var message = FriendViewModels.FirstOrDefault(x => x.Id == y.To)?
                .Messages.FirstOrDefault(x => x.MessageId == y.MessageId);

                if (message != null)
                {
                    message.SendSuccess = false;
                }
            });

            WeakReferenceMessenger.Default.Register<ChatViewModel, dynamic, string>(this, "message-send-finished", (x, y) =>
            {
                var message = FriendViewModels.FirstOrDefault(x => x.Id == y.To)?
                .Messages.FirstOrDefault(x => x.MessageId == y.MessageId);

                if (message != null)
                {
                    message.Sending = false;
                }
            });
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
                try
                {
                    //加载历史100条聊天记录 //TODO与服务器聊天记录做比较
                    var records = await FetchChatRecordsFromRemoteAsync(tempFriend.Id, DateTime.Now);
                    if (records != null && records.Count() > 0)
                    {
                        foreach (var record in records)
                        {
                            tempFriend.AddMessage(record);
                        }
                    }

                    tempFriend.LoadedChatRecord = true;
                }
                catch (Exception ex)
                {
                    //TODO log记录
                }
            }
        }

        /// <summary>
        /// 从服务器拉取聊天记录
        /// </summary>
        /// <param name="friendId">好友id</param>
        /// <param name="fromTime">拉取时间</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<IEnumerable<MessageViewModel>> FetchChatRecordsFromRemoteAsync(long friendId, DateTime fromTime)
        {
            var resp = await _contextAPIService.GetAsync<FriendChatRecordResponseDto>(string.Format(Const.FETCH_FRIEND_CHATRECORDS,
                friendId, fromTime.ToString("yyyy-MM-dd HH:mm:ss.fff")));
            if (resp == null)
            {
                _growlHelper.Warning("获取聊天记录失败");
                throw new Exception("拉取聊天记录异常");
            }

            if (resp.Records == null || resp.Records.Count() <= 0)
            {
                return default;
            }

            var localImageRecords = resp.Records.Where(x => x.MessageRecordType == MessageRecordType.Image
                && x.ImageType == ImageType.Local);
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

            var netImageRecords = resp.Records.Where(x => x.MessageRecordType == MessageRecordType.Image
                && x.ImageType == ImageType.Network);
            if (netImageRecords != null && netImageRecords.Count() > 0)
            {
                foreach (var record in netImageRecords)
                {
                    record.Bytes = await _contextAPIService.DownloadNetworkImageAsync(record.Message);
                }
            }

            List<MessageViewModel> messages = new List<MessageViewModel>();
            foreach (var record in resp.Records.OrderBy(x => x.SendTime))
            {
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
            var tempFriend = FriendViewModel;
            if (string.IsNullOrEmpty(TextMessage))
                return;

            if (TextMessage.Length > 500)
            {
                _growlHelper.Warning("文本长度不可超过500");
                return;
            }

            var viewModel = new TextMessageViewModel()
            {
                FromSelf = true,
                Text = textMessage,
                SendTime = DateTime.Now,
                From = App.UserProfile.UserId,
                To = tempFriend.Id,
                Sending = true,
            };

            tempFriend.AddMessage(viewModel);
            await SendTextMessageByVm(viewModel);
        }

        private async Task SendTextMessageByVm(TextMessageViewModel viewModel)
        {
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
            var tempFriend = FriendViewModel;

            var viewModel = new ImageMessageViewModel()
            {
                FromSelf = true,
                ImageType = ImageType.Network,
                SendTime = DateTime.Now,
                From = App.UserProfile.UserId,
                To = tempFriend.Id,
                Sending = true,
                RemoteUrl = routedEventArgs.Emoji.Url
            };

            var bytes = await _contextAPIService.DownloadNetworkImageAsync(viewModel.RemoteUrl);
            viewModel.Source = _fileHelper.ByteArrayToBitmapImage(bytes);
            tempFriend.AddMessage(viewModel);
            await SendEmojiByVmAsync(viewModel);
        }

        private async Task SendEmojiByVmAsync(ImageMessageViewModel viewModel)
        {
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
        private async Task SendImageFileAsync(FileInfo fileInfo, FriendViewModel friend)
        {
            var tempFriendViewModel = FriendViewModel;
            var viewModel = new ImageMessageViewModel()
            {
                FromSelf = true,
                ImageType = ImageType.Local,
                SendTime = DateTime.Now,
                From = App.UserProfile.UserId,
                Location = fileInfo.FullName,
                To = friend.Id,
                Sending = true
            };

            viewModel.Source = _fileHelper.ByteArrayToBitmapImage(await File.ReadAllBytesAsync(fileInfo.FullName));
            tempFriendViewModel.AddMessage(viewModel);
            var fileId = await UploadFileAsync(fileInfo, friend.Id);
            viewModel.FileId = fileId;
            await SendImageByVmAsync(viewModel);
        }

        private async Task SendImageByVmAsync(ImageMessageViewModel viewModel)
        {
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
        private async Task SendFileAsync(FileInfo fileInfo, FriendViewModel friend)
        {
            var tempFriendViewModel = FriendViewModel;
            var viewModel = new FileMessageViewModel(fileInfo.Name)
            {
                FromSelf = true,
                FileSize = fileInfo.Length,
                SendTime = DateTime.Now,
                From = App.UserProfile.UserId,
                Location = fileInfo.FullName,
                To = friend.Id,
                Sending = true
            };

            tempFriendViewModel.AddMessage(viewModel);
            var fileId = await UploadFileAsync(fileInfo, friend.Id);
            viewModel.FileId = fileId;
            await SendFileByVmAsync(viewModel);
        }

        private async Task SendFileByVmAsync(FileMessageViewModel viewModel)
        {
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

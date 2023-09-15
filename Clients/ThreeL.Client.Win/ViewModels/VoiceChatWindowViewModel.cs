using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using ThreeL.Client.Shared.Utils;
using ThreeL.Infra.Core.Metadata;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class VoiceChatWindowViewModel : ObservableObject
    {
        private RelationViewModel _current;
        public RelationViewModel Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        private bool _started;
        public bool Started
        {
            get => _started;
            set => SetProperty(ref _started, value);
        }

        private string _wattingText;
        public string WattingText
        {
            get => _wattingText;
            set => SetProperty(ref _wattingText, value);
        }

        private string _loadingText;
        public string LoadingText
        {
            get => _loadingText;
            set => SetProperty(ref _loadingText, value);
        }

        public string VoiceChatKey { get; set; }

        private Task _loadingTask;

        public AsyncRelayCommand CancelCommandAsync { get; set; }

        private readonly TcpSuperSocketClient _tcpSuperSocketClient;
        private readonly SequenceIncrementer _sequenceIncrementer;
        public VoiceChatWindowViewModel(TcpSuperSocketClient tcpSuperSocketClient, SequenceIncrementer sequenceIncrementer)
        {
            _tcpSuperSocketClient = tcpSuperSocketClient;
            CancelCommandAsync = new AsyncRelayCommand(CancelAsync);
            _loadingTask = Task.Run(async () =>
            {
                while (true)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (LoadingText?.Length >= 3)
                        {
                            LoadingText = ".";
                        }
                        else
                        {
                            LoadingText = $"{LoadingText}.";
                        }
                    });

                    await Task.Delay(1000);
                }
            });
            _sequenceIncrementer = sequenceIncrementer;
        }

        private async Task CancelAsync() 
        {
            var packet = new Packet<FinishVoiceChatMessage>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.FinishVoiceChat,
                Body =  new FinishVoiceChatMessage() 
                {
                    Chatkey = VoiceChatKey,
                    Action = VoiceChatStatus.Canceled,
                    To = Current.Id,
                    IsGroup = false
                }
            };

            await _tcpSuperSocketClient.SendBytesAsync(packet.Serialize());
            //无论结果如何，关闭本地udp通信，结束通话
        }

        ~VoiceChatWindowViewModel()
        {
            _loadingTask.Dispose();
        }
    }
}

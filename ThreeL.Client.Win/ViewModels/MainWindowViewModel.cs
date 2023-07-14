using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;

namespace ThreeL.Client.Win.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private string _remoteAddress;
        public string RemoteAddress
        {
            get => _remoteAddress;
            set => SetProperty(ref _remoteAddress, value);
        }

        private ushort _remotePort;
        public ushort RemotePort
        {
            get => _remotePort;
            set => SetProperty(ref _remotePort, value);
        }

        public AsyncRelayCommand ConnectCommandAsync { get; set; }
        public AsyncRelayCommand SendTextCommandAsync { get; set; }

        private readonly TcpSuperSocketClient _tcpSuperSocket;
        private readonly UdpSuperSocketClient _udpSuperSocket;

        public MainWindowViewModel(TcpSuperSocketClient tcpSuperSocket, UdpSuperSocketClient udpSuperSocket)
        {
            ConnectCommandAsync = new AsyncRelayCommand(ConnectAsync);
            SendTextCommandAsync = new AsyncRelayCommand(SendTextAsync);
            _tcpSuperSocket = tcpSuperSocket;
            RemoteAddress = "127.0.0.1";
            RemotePort = 4040;
            _udpSuperSocket = udpSuperSocket;
        }

        private async Task ConnectAsync()
        {
            var result = await _tcpSuperSocket.ConnectAsync(RemoteAddress, RemotePort);
        }

        private async Task SendTextAsync()
        {
            await _udpSuperSocket.SendBytes(new IPEndPoint(IPAddress.Parse("127.0.0.1"),11887),new Packet<TextMessage>()
            {
                Checkbit = 570,
                Sequence = 250,
                MessageType = Shared.SuperSocket.Metadata.MessageType.Text,
                Body = new TextMessage
                {
                    Text = "小刘下午好",
                }
            }.Serialize());
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Database;
using ThreeL.Shared.SuperSocket.Client;

namespace ThreeL.Client.Win.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public AsyncRelayCommand LoginCommandAsync { get; set; }
        public AsyncRelayCommand SendTextCommandAsync { get; set; }

        private readonly IConfiguration _configuration;
        private readonly ClientSqliteContext _clientSqliteContext;
        private readonly TcpSuperSocketClient _tcpSuperSocket; //通讯服务器socket
        private readonly UdpSuperSocketClient _udpSuperSocket; //本地udp通讯socket

        public MainWindowViewModel(TcpSuperSocketClient tcpSuperSocket,
                                   UdpSuperSocketClient udpSuperSocket,
                                   ClientSqliteContext clientSqliteContext,
                                   IConfiguration configuration)
        {
            LoginCommandAsync = new AsyncRelayCommand(LoginAsync);
            SendTextCommandAsync = new AsyncRelayCommand(SendTextAsync);
            _clientSqliteContext = clientSqliteContext;
            _configuration = configuration;
            _tcpSuperSocket = tcpSuperSocket;
            _udpSuperSocket = udpSuperSocket;
        }

        private async Task LoginAsync()
        {
            //var result = await _tcpSuperSocket.ConnectAsync(_configuration, RemotePort);
        }

        private async Task SendTextAsync()
        {
            //await _tcpSuperSocket.SendBytes(new Packet<TextMessage>()
            //{
            //    Checkbit = 570,
            //    Sequence = 250,
            //    MessageType = Shared.SuperSocket.Metadata.MessageType.Text,
            //    Body = new TextMessage
            //    {
            //        Text = "小刘下午好",
            //    }
            //}.Serialize());
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using ThreeL.Client.Mobile.Helper;
using ThreeL.Client.Shared.Configurations;
using ThreeL.Client.Shared.Dtos.ContextAPI;
using ThreeL.Client.Shared.Services;
using ThreeL.Client.Shared.Utils;
using ThreeL.Shared.SuperSocket.Cache;
using ThreeL.Shared.SuperSocket.Client;

namespace ThreeL.Client.Mobile.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        public AsyncRelayCommand LoadedCommandAsync { get; set; }

        private readonly TcpSuperSocketClient _tcpSuperSocket;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly PacketWaiter _packetWaiter;
        private readonly SocketServerOptions _socketServerOptions;
        private readonly WarningHelper _warningHelper;
        private readonly ContextAPIService _contextAPIService;

        public ChatViewModel(TcpSuperSocketClient tcpSuperSocket, 
                             SequenceIncrementer sequenceIncrementer,
                             PacketWaiter packetWaiter,
                             IOptions<SocketServerOptions> socketServerOptions, 
                             WarningHelper warningHelper,
                             ContextAPIService contextAPIService)
        {
            _tcpSuperSocket = tcpSuperSocket;
            _warningHelper = warningHelper;
            _socketServerOptions = socketServerOptions.Value;
            _packetWaiter = packetWaiter;
            _sequenceIncrementer = sequenceIncrementer;
            _contextAPIService = contextAPIService;
            LoadedCommandAsync = new AsyncRelayCommand(LoadAsync);
        }

        private async Task LoadAsync()
        {
            var resp = await _contextAPIService.GetAsync<IEnumerable<FriendDto>>(Const.FETCH_FRIENDS);
        }
    }
}

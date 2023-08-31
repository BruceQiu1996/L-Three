using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ThreeL.Client.Shared.Utils;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Client;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.ViewModels
{
    public class InviteFriendsIntoGroupViewModel : ObservableObject
    {
        private ObservableCollection<RelationViewModel> toBeInviteRelationViewModels;
        public ObservableCollection<RelationViewModel> ToBeInviteRelationViewModels
        {
            get => toBeInviteRelationViewModels;
            set => SetProperty(ref toBeInviteRelationViewModels, value);
        }

        private ObservableCollection<RelationViewModel> leftToBeInviteRelationViewModels;
        public ObservableCollection<RelationViewModel> LeftToBeInviteRelationViewModels
        {
            get => leftToBeInviteRelationViewModels;
            set => SetProperty(ref leftToBeInviteRelationViewModels, value);
        }

        private RelationViewModel leftToBeInviteRelationViewModel;
        public RelationViewModel LeftToBeInviteRelationViewModel
        {
            get => leftToBeInviteRelationViewModel;
            set => SetProperty(ref leftToBeInviteRelationViewModel, value);
        }

        private ObservableCollection<RelationViewModel> rightToBeInviteRelationViewModels;
        public ObservableCollection<RelationViewModel> RightToBeInviteRelationViewModels
        {
            get => rightToBeInviteRelationViewModels;
            set => SetProperty(ref rightToBeInviteRelationViewModels, value);
        }

        private RelationViewModel rightToBeInviteRelationViewModel;
        public RelationViewModel RightToBeInviteRelationViewModel
        {
            get => rightToBeInviteRelationViewModel;
            set => SetProperty(ref rightToBeInviteRelationViewModel, value);
        }

        public int GroupId { get; set; }
        public RelayCommand RightSelectFriendCommand { get; set; }
        public RelayCommand LeftSelectFriendCommand { get; set; }
        public AsyncRelayCommand InviteFriendsCommandAsync { get; set; }

        private readonly TcpSuperSocketClient _tcpSuperSocketClient;
        private readonly SequenceIncrementer _sequenceIncrementer;
        private readonly GrowlHelper _growlHelper;

        public InviteFriendsIntoGroupViewModel(TcpSuperSocketClient tcpSuperSocketClient, SequenceIncrementer sequenceIncrementer, GrowlHelper growlHelper)
        {
            _tcpSuperSocketClient = tcpSuperSocketClient;
            RightToBeInviteRelationViewModels = new ObservableCollection<RelationViewModel>();
            RightSelectFriendCommand = new RelayCommand(RightSelectFriend);
            LeftSelectFriendCommand = new RelayCommand(LeftSelectFriend);
            InviteFriendsCommandAsync = new AsyncRelayCommand(InviteFriendsAsync);
            _sequenceIncrementer = sequenceIncrementer;
            _growlHelper = growlHelper;
        }

        private void LeftSelectFriend()
        {
            if (LeftToBeInviteRelationViewModel != null)
            {
                RightToBeInviteRelationViewModels.Add(LeftToBeInviteRelationViewModel);
                LeftToBeInviteRelationViewModels.Remove(LeftToBeInviteRelationViewModel);
            }
        }

        private void RightSelectFriend()
        {
            if (RightToBeInviteRelationViewModel != null)
            {
                LeftToBeInviteRelationViewModels.Add(RightToBeInviteRelationViewModel);
                RightToBeInviteRelationViewModels.Remove(RightToBeInviteRelationViewModel);
            }
        }

        private async Task InviteFriendsAsync() 
        {
            if (RightToBeInviteRelationViewModels.Count <= 0)
                return;

            var packet = new Packet<InviteMembersIntoGroupCommand>()
            {
                Sequence = _sequenceIncrementer.GetNextSequence(),
                MessageType = MessageType.InviteFriendsIntoGroup,
                Body = new InviteMembersIntoGroupCommand() 
                {
                    Friends = string.Join(",",RightToBeInviteRelationViewModels.Select(x=>x.Id)),
                    GroupId = GroupId
                }
            };

            var sendResult = await _tcpSuperSocketClient.SendBytesAsync(packet.Serialize());
            if (!sendResult)
            {
                _growlHelper.Warning("邀请好友失败，请稍后再试");
            }
        }
    }
}

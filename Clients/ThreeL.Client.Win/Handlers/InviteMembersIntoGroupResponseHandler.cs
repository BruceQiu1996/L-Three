using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Client.Win.Helpers;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class InviteMembersIntoGroupResponseHandler : AbstractMessageHandler
    {
        private readonly GrowlHelper _growlHelper;
        public InviteMembersIntoGroupResponseHandler(GrowlHelper growlHelper) : base(MessageType.InviteFriendsIntoGroupResponse)
        {
            _growlHelper = growlHelper;
        }
        public async override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var resp = message as Packet<InviteMembersIntoGroupCommandResponse>;
            if (!resp.Body.Result) 
            {
                _growlHelper.Warning(resp.Body.Message);
            }

            if (App.UserProfile.UserId != resp.Body.InviterId) 
            {
                WeakReferenceMessenger.Default.Send(resp.Body, "message-invite-group");
            }
        }
    }
}

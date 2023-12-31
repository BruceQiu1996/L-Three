﻿using CommunityToolkit.Mvvm.Messaging;
using SuperSocket;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Client.Win.Handlers
{
    public class FileMessageResponseHandler : ClientMessageHandler
    {
        public FileMessageResponseHandler() : base(MessageType.FileResp)
        {
        }

        public override Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<FileMessageResponse>;
            WeakReferenceMessenger.Default.Send<FromToMessageResponse, string>(packet.Body, "message-receive");

            return Task.CompletedTask;
        }
    }
}

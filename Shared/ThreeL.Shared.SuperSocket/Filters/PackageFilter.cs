using SuperSocket.ProtoBase;
using System.Buffers;
using ThreeL.Infra.SuperSocket.Filters;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Commands;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Shared.SuperSocket.Filters
{
    public class PackageFilter : AbstractPackageFilter<IPacket>
    {
        public PackageFilter() : base(IPacket.HeaderSize)
        {

        }

        public override IPacket ResolvePackage(ref ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);

            if (reader.TryReadBigEndian(out int checkbit) &&
                reader.TryReadBigEndian(out long sequence) &&
                reader.TryReadBigEndian(out int length) &&
                reader.TryRead(out byte msgType))
            {
                //针对不同的消息处理，订阅应用层的事件
                var seq = buffer.Slice(IPacket.HeaderSize);
                IPacket packet = (MessageType)msgType switch
                {
                    MessageType.Text => new Packet<TextMessage>(),
                    MessageType.TextResp => new Packet<TextMessageResponse>(),
                    MessageType.Image => new Packet<ImageMessage>(),
                    MessageType.ImageResp => new Packet<ImageMessageResponse>(),
                    MessageType.File => new Packet<FileMessage>(),
                    MessageType.FileResp => new Packet<FileMessageResponse>(),
                    MessageType.Withdraw => new Packet<WithdrawMessage>(),
                    MessageType.WithdrawResp => new Packet<WithdrawMessageResponse>(),
                    MessageType.Login => new Packet<LoginCommand>(),
                    MessageType.LoginResponse => new Packet<LoginCommandResponse>(),
                    MessageType.RequestForUserEndpoint => new Packet<RequestForUserEndpointCommand>(),
                    MessageType.RequestForUserEndpointResponse => new Packet<RequestForUserEndpointCommandResponse>(),
                    MessageType.AddFriend => new Packet<AddFriendCommand>(),
                    MessageType.AddFriendResponse => new Packet<AddFriendCommandResponse>(),
                    MessageType.ReplyAddFriend => new Packet<ReplyAddFriendCommand>(),
                    MessageType.ReplyAddFriendResponse => new Packet<ReplyAddFriendCommandResponse>(),
                    _ => new Packet<TextMessage>()
                };

                packet.Checkbit = checkbit;
                packet.Sequence = sequence;
                packet.Length = length;
                packet.MessageType = (MessageType)msgType;
                packet.Deserialize(seq.ToArray());

                return packet;
            }
            else
            {
                return null;
            }
        }
    }
}

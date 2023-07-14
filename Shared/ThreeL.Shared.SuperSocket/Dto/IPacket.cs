using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto
{
    public interface IPacket
    {
        internal static byte HeaderSize { get; } = 4 + 8 + 4 + 1;
        public int Checkbit { get; set; }
        public long Sequence { get; set; }
        public int Length { get; set; }
        public MessageType MessageType { get; set; }
        void Deserialize(byte[] bodyData);
        byte[] Serialize();
    }
}

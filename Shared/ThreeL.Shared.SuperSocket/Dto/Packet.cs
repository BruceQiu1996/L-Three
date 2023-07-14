using System.Runtime.InteropServices;
using ThreeL.Infra.Core.Array;
using ThreeL.Infra.Core.Serializer;
using ThreeL.Shared.SuperSocket.Metadata;

namespace ThreeL.Shared.SuperSocket.Dto
{
    [StructLayout(LayoutKind.Sequential)]
    public class Packet<TBody> : IPacket where TBody : IMessage
    { 
        /// <summary>
        /// 包头标志，用于校验 4byte
        /// </summary>
        public int Checkbit { get; set; }
        public long Sequence { get ; set ; }

        #region Head
        public int Length { get; set; }
        public MessageType MessageType { get; set; }
        #endregion

        public TBody? Body { get; set; }
        
        public void Deserialize(byte[] bodyData)
        {
            Body = ProtoBufSerializer.DeSerialize<TBody>(bodyData);
        }

        public byte[] Serialize()
        {
            Checkbit = 0x1F;
            Length = IPacket.HeaderSize;
            var bodyArray = ProtoBufSerializer.Serialize(Body);
            Length += bodyArray.Length;
            byte[] result = new byte[Length];
            result.AddInt32(0,Checkbit);
            result.AddInt64(4, Sequence);
            result.AddInt32(12, Length);
            result.AddInt8(16, (byte)MessageType);
            Buffer.BlockCopy(bodyArray, 0, result,17, bodyArray.Length);

            return result;
        }
    }
}

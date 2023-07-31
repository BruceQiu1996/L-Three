using System.Collections.Concurrent;
using ThreeL.Shared.SuperSocket.Dto;

namespace ThreeL.Shared.SuperSocket.Cache
{
    public class PacketWaitContainer
    {
        private readonly ConcurrentDictionary<string, IPacket>
            _waitPacket = new ConcurrentDictionary<string, IPacket>();

        public void Add(string key, IPacket packet, bool needExist)
        {
            if (!needExist)
            {
                _waitPacket.AddOrUpdate(key, packet, (k, v) => packet);
            }
            else
            {
                if (_waitPacket.ContainsKey(key))
                {
                    _waitPacket.AddOrUpdate(key, packet, (k, v) => packet);
                }
            }
        }

        public IPacket Get(string key) 
        {
            _waitPacket.TryGetValue(key, out IPacket value);

            return value;
        }

        public void Remove(string key)
        {
            _waitPacket.TryRemove(key, out _);
        }
    }
}

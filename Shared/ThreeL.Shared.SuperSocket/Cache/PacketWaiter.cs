using ThreeL.Shared.SuperSocket.Dto;

namespace ThreeL.Shared.SuperSocket.Cache
{
    public class PacketWaiter
    {
        private readonly PacketWaitContainer _waitContainer;
        public PacketWaiter(PacketWaitContainer waitContainer)
        {
            _waitContainer = waitContainer;
        }

        public void AddWaitPacket(string key, IPacket packet, bool needExist)
        {
            _waitContainer.Add(key, packet, needExist);
        }

        public async Task<IPacket> GetAnswerPacketAsync(string key, CancellationTokenSource cancellationToken)
        {
            try
            {
                var result = await Task.Run(() =>
                {
                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested) 
                        {
                            return null;
                        }
                        var packet = _waitContainer.Get(key);
                        if (packet != null)
                        {
                            return packet;
                        }
                    }
                }, cancellationToken.Token);

                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                _waitContainer.Remove(key);
            }
        }
    }
}

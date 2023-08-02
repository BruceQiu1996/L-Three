using ThreeL.Shared.SuperSocket.Dto;

namespace ThreeL.Shared.SuperSocket.Cache
{
    public class PacketWaiter
    {
        /// <summary>
        /// 使tcp server能够等待客户端的回复
        /// 因为tcp server是无状态的，所以需要缓存客户端的回复
        /// 分布式场景下，需要使用redis等缓存
        /// </summary>
        private readonly PacketWaitContainer _waitContainer;
        public PacketWaiter(PacketWaitContainer waitContainer)
        {
            _waitContainer = waitContainer;
        }

        public void AddWaitPacket(string key, IPacket packet, bool needExist)
        {
            _waitContainer.Add(key, packet, needExist);
        }

        public async Task<T> GetAnswerPacketAsync<T>(string key, int timeOut = 5) where T : class, IPacket
        {
            try
            {
                CancellationTokenSource cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeOut));
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

                return result as T;
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

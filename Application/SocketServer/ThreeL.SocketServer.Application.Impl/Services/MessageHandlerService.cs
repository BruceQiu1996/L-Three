using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.Infra.Redis;
using ThreeL.SocketServer.Application.Contract.Configurations;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.Application.Impl.Services
{
    public class MessageHandlerService : IMessageHandlerService, IAppService
    {
        private readonly IRedisProvider _redisProvider;
        public MessageHandlerService(IRedisProvider redisProvider)
        {
            _redisProvider = redisProvider;
        }

        public async Task<bool> IsFriendAsync(long u1, long u2)
        {
            var e1 = await _redisProvider.SetIsMemberAsync(Const.FRIEND_RELATIONS, $"{u1}-{u2}");
            var e2 = await _redisProvider.SetIsMemberAsync(Const.FRIEND_RELATIONS, $"{u2}-{u1}");
            return e1 || e2; //TODO grpc 查询数据库
        }
    }
}

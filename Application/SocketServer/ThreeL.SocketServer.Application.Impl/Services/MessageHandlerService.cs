using ThreeL.Infra.Redis;
using ThreeL.Shared.Application;
using ThreeL.SocketServer.Application.Contract;
using ThreeL.SocketServer.Application.Contract.Services;

namespace ThreeL.SocketServer.Application.Impl.Services
{
    public class MessageHandlerService : IMessageHandlerService
    {
        private readonly IRedisProvider _redisProvider;
        private readonly IContextAPIGrpcService _contextAPIGrpcService;
        public MessageHandlerService(IRedisProvider redisProvider, IContextAPIGrpcService contextAPIGrpcService)
        {
            _redisProvider = redisProvider;
            _contextAPIGrpcService = contextAPIGrpcService;
        }

        public async Task<bool> IsValidRelationAsync(long u1, long u2, bool isGroup, string token)
        {
            if (!isGroup)
            {
                var e1 = await _redisProvider.SetIsMemberAsync(CommonConst.FRIEND_RELATION, $"{u1}-{u2}");
                var e2 = await _redisProvider.SetIsMemberAsync(CommonConst.FRIEND_RELATION, $"{u2}-{u1}");
                if (e1 || e2)
                    return true;
            }

            if (isGroup)
            {
                var isInGroup = await _redisProvider.SetIsMemberAsync(string.Format(CommonConst.GROUP, u2), u1.ToString());
                if (isInGroup) return true;
            }

            var result = await _contextAPIGrpcService.ValidateRelation(new ValidateRelationRequest()
            {
                To = u2,
                IsGroup = isGroup
            }, token);

            return result.Result;
        }
    }
}

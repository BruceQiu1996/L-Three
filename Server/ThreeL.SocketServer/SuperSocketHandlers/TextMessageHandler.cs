﻿using SuperSocket;
using ThreeL.Infra.Redis;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Dto.Message;
using ThreeL.Shared.SuperSocket.Handlers;
using ThreeL.Shared.SuperSocket.Metadata;
using ThreeL.SocketServer.Application.Contract.Configurations;

namespace ThreeL.SocketServer.SuperSocketHandlers
{
    public class TextMessageHandler : AbstractMessageHandler
    {
        private readonly ServerAppSessionManager<ChatSession> _sessionManager;
        private readonly IRedisProvider _redisProvider;

        public TextMessageHandler(ServerAppSessionManager<ChatSession> sessionManager, IRedisProvider redisProvider) : base(MessageType.Text)
        {
            _redisProvider = redisProvider;
            _sessionManager = sessionManager;
        }

        public override async Task ExcuteAsync(IAppSession appSession, IPacket message)
        {
            var packet = message as Packet<TextMessage>;
            var resp = new Packet<TextMessageResponse>()
            {
                Sequence = packet.Sequence,
                Checkbit = packet.Checkbit,
                MessageType = MessageType.TextResp,
            };

            var body = new TextMessageResponse();
            resp.Body = body;

            if (packet.Body.From != packet.Body.To)
            {
                var e1 = await _redisProvider.SetIsMemberAsync(Const.FRIEND_RELATIONS, $"{packet.Body.From}-{packet.Body.To}");
                var e2 = await _redisProvider.SetIsMemberAsync(Const.FRIEND_RELATIONS, $"{packet.Body.To}-{packet.Body.From}");
                if ((e1 || e2) == false)
                {
                    body.Result = false;
                    body.Message = "好友关系异常";
                    await appSession.SendAsync(resp.Serialize());
                    return;
                }
            }

            body.Result = true;
            body.Text = packet.Body.Text;
            body.From = packet.Body.From;
            body.To = packet.Body.To;
            body.SendTime = packet.Body.SendTime;
            //分发给发送者和接收者
            var fromSessions = _sessionManager.TryGet(resp.Body.From);
            var toSessions = _sessionManager.TryGet(resp.Body.To);
            if (fromSessions != null)
            {
                foreach (var item in fromSessions)
                {
                    await (item! as IAppSession).SendAsync(resp.Serialize());
                }
            }

            if (packet.Body.From != packet.Body.To)
            {
                if (toSessions != null)
                {
                    foreach (var item in toSessions)
                    {
                        await (item! as IAppSession).SendAsync(resp.Serialize());
                    }
                }
            }
        }
    }
}

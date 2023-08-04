using Microsoft.Extensions.Hosting;
using SuperSocket;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ThreeL.Shared.SuperSocket.Dto;
using ThreeL.Shared.SuperSocket.Filters;
using ThreeL.Shared.SuperSocket.Handlers;

namespace ThreeL.Client.Win.BackgroundService
{
    public class UdpServerRunningService : IHostedService
    {
        private readonly MessageHandlerDispatcher _handlerDispatcher;
        private readonly IPEndPoint _iPEndPoint;
        private IHost? _udpServerHost;

        public UdpServerRunningService(MessageHandlerDispatcher handlerDispatcher, IPEndPoint iPEndPoint)
        {
            _iPEndPoint = iPEndPoint;
            _handlerDispatcher = handlerDispatcher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _udpServerHost = SuperSocketHostBuilder
                .Create<IPacket, PackageFilter>().UsePackageHandler(async (session, package) =>
                {
                    await _handlerDispatcher.DispatcherMessageHandlerAsync(session, package);
                }).ConfigureSuperSocket(options =>
                {
                    options.Name = "window udp server";
                    options.Listeners = new List<ListenOptions> {
                                new ListenOptions
                                {
                                    Ip = _iPEndPoint.Address.ToString(),
                                    Port = _iPEndPoint.Port
                                }
                            };
                }).UseUdp().Build();

            await _udpServerHost!.RunAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _udpServerHost!.StopAsync(cancellationToken);
        }
    }
}

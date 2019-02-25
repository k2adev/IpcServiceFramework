using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace K2adev.IpcServiceFramework.Tcp
{
    public class TcpIpcServiceEndpoint<TContract> : IpcServiceEndpoint<TContract>
        where TContract : class
    {
        private readonly ILogger<TcpIpcServiceEndpoint<TContract>> _logger;

        public int Port { get; private set; }

        private readonly TcpListener _listener;

        public TcpIpcServiceEndpoint(String name, IServiceProvider serviceProvider, IPAddress ipEndpoint, int port)
            : base(name, serviceProvider, serviceProvider.GetRequiredService<IpcServiceOptions>())
        {
            _listener = new TcpListener(ipEndpoint, port);
            _logger = serviceProvider.GetService<ILogger<TcpIpcServiceEndpoint<TContract>>>();
            Port = port;
        }

        public TcpIpcServiceEndpoint(String name, IServiceProvider serviceProvider, IPAddress ipEndpoint)
            : this(name, serviceProvider, ipEndpoint, 0)
        {
        }

        public override Task ListenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IpcServiceOptions options = ServiceProvider.GetRequiredService<IpcServiceOptions>();

            _listener.Start();

            // If port is dynamically assigned, get the port number after start
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

            cancellationToken.Register(() =>
            {
                _listener.Stop();
            });

            return Task.Run(async () =>
            {
                try
                {
                    _logger.LogDebug($"Endpoint '{Name}' listening on port {Port}...");
                    while (true)
                    {
                        TcpClient client = await _listener.AcceptTcpClientAsync();
                        Stream server = client.GetStream();

                        await ProcessAsync(server, _logger, cancellationToken);
                    }
                }
                catch when (cancellationToken.IsCancellationRequested)
                { }
            });
        }
    }
}
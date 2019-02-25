﻿using IpcServiceSample.ServiceContracts;
using K2adev.IpcServiceFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IpcServiceSample.ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // configure DI
            IServiceCollection services = ConfigureServices(new ServiceCollection());

            // build and run service host
            IIpcServiceHost host = new IpcServiceHostBuilder(services.BuildServiceProvider())
                .AddNamedPipeEndpoint<IComputingService>("computingEndpoint", "pipeName")
                .AddTcpEndpoint<ISystemService>("systemEndpoint", IPAddress.Loopback, 45684)
                .Build();

            var source = new CancellationTokenSource();
            Task.WaitAll(host.RunAsync(source.Token), Task.Run(() =>
            {
                Console.WriteLine("Press any key to shutdown.");
                Console.ReadKey();
                source.Cancel();
            }));

            Console.WriteLine("Server stopped.");
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .AddIpc(builder =>
                {
                    builder
                        .AddNamedPipe(options =>
                        {
                            options.ThreadCount = 2;
                            options.GZipCompressionEnabled = true;
                            options.Aes256EncryptionEnabled = true;
                        })
                        .AddTcp(options => 
                        {
                            options.GZipCompressionEnabled = true;
                            options.Aes256EncryptionEnabled = true;
                        })
                        .AddService<IComputingService, ComputingService>()
                        .AddService<ISystemService, SystemService>();
                });
        }
    }
}

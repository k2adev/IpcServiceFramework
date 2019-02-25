using Microsoft.Extensions.DependencyInjection;
using System;

namespace K2adev.IpcServiceFramework
{
    public static class TcpIpcServiceCollectionExtensions
    {
        public static IIpcServiceBuilder AddTcp(this IIpcServiceBuilder builder)
        {
            return builder;
        }

        public static IIpcServiceBuilder AddTcp(this IIpcServiceBuilder builder, Action<IpcServiceOptions> configure)
        {
            var options = new IpcServiceOptions();
            configure?.Invoke(options);

            builder.Services
                .AddSingleton(options)
                ;

            return builder;
        }
    }
}

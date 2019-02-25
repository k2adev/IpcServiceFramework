using Microsoft.Extensions.DependencyInjection;
using System;

namespace K2adev.IpcServiceFramework
{
    public static class NamedPipeIpcServiceCollectionExtensions
    {
        public static IIpcServiceBuilder AddNamedPipe(this IIpcServiceBuilder builder)
        {
            return builder.AddNamedPipe(null);
        }

        public static IIpcServiceBuilder AddNamedPipe(this IIpcServiceBuilder builder, Action<IpcServiceOptions> configure)
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
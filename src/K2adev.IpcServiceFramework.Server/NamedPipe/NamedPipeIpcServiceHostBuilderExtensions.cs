﻿using K2adev.IpcServiceFramework.NamedPipe;

namespace K2adev.IpcServiceFramework
{
    public static class NamedPipeIpcServiceHostBuilderExtensions
    {
        public static IpcServiceHostBuilder AddNamedPipeEndpoint<TContract>(this IpcServiceHostBuilder builder,
            string name, string pipeName)
            where TContract: class
        {
            return builder.AddEndpoint(new NamedPipeIpcServiceEndpoint<TContract>(name, builder.ServiceProvider, pipeName));
        }
    }
}

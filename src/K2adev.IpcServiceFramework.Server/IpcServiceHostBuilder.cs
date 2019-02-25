using System;
using System.Collections.Generic;

namespace K2adev.IpcServiceFramework
{
    public class IpcServiceHostBuilder
    {
        private readonly List<IpcServiceEndpoint> _endpoints = new List<IpcServiceEndpoint>();

        public IpcServiceHostBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public IpcServiceHostBuilder AddEndpoint(IpcServiceEndpoint endpoint)
        {
            _endpoints.Add(endpoint);
            return this;
        }

        public IIpcServiceHost Build()
        {
            return new IpcServiceHost(_endpoints, ServiceProvider);
        }
    }
}

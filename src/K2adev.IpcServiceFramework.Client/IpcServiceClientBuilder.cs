using System;

namespace K2adev.IpcServiceFramework
{
    public class IpcServiceClientBuilder<TInterface>
        where TInterface : class
    {
        private IIpcMessageSerializer _serializer = new IpcMessageSerializer();
        private Func<IIpcMessageSerializer, IpcServiceClient<TInterface>> _factory = null;

        public IpcServiceClientBuilder<TInterface> WithIpcMessageSerializer(IIpcMessageSerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        public IpcServiceClientBuilder<TInterface> SetFactory(Func<IIpcMessageSerializer, IpcServiceClient<TInterface>> factory)
        {
            _factory = factory;
            return this;
        }

        public IpcServiceClient<TInterface> Build()
        {
            if (_factory == null)
            {
                throw new InvalidOperationException("Client factory is not set.");
            }

            return _factory(_serializer);
        }
    }
}

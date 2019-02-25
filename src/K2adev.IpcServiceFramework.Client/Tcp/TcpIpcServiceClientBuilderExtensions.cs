using System.Net;
using K2adev.IpcServiceFramework.Tcp;

namespace K2adev.IpcServiceFramework
{
    public static class TcpIpcServiceClientBuilderExtensions
    {
        public static IpcServiceClientBuilder<TInterface> UseTcp<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, IpcServiceOptions options, IPAddress serverIp, int serverPort)
            where TInterface : class
        {
            builder.SetFactory((serializer) => new TcpIpcServiceClient<TInterface>(serializer, options, serverIp, serverPort));

            return builder;
        }
    }
}

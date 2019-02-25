using K2adev.IpcServiceFramework.NamedPipe;

namespace K2adev.IpcServiceFramework
{
    public static class NamedPipeIpcServiceClientBuilderExtensions
    {
        public static IpcServiceClientBuilder<TInterface> UseNamedPipe<TInterface>(
            this IpcServiceClientBuilder<TInterface> builder, string pipeName, IpcServiceOptions options)
            where TInterface : class
        {
            builder.SetFactory((serializer) => new NamedPipeIpcServiceClient<TInterface>(serializer, pipeName, options));

            return builder;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System;

namespace K2adev.IpcServiceFramework
{
    public static class IpcServiceServiceCollectionExtensions
    {
        [Obsolete("Use services.AddIpc(builder => { ... }) instead.")]
        public static IIpcServiceBuilder AddIpc(this IServiceCollection services)
        {
            IIpcServiceBuilder builder = null;
            services.AddIpc(x => builder = x);
            return builder;
        }

        public static IServiceCollection AddIpc(this IServiceCollection services, Action<IIpcServiceBuilder> configAction)
        {
            services
                .AddLogging()
                .AddScoped<IIpcMessageSerializer, IpcMessageSerializer>();

            var builder = new IpcServiceBuilder(services);
            configAction?.Invoke(builder);
            return services;
        }
    }
}

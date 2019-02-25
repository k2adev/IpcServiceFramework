using K2adev.IpcServiceFramework.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace K2adev.IpcServiceFramework
{
    public abstract class IpcServiceEndpoint
    {
        protected IpcServiceEndpoint(string name, IServiceProvider serviceProvider)
        {
            Name = name;
            ServiceProvider = serviceProvider;
        }

        public string Name { get; }
        public IServiceProvider ServiceProvider { get; }

        public abstract Task ListenAsync(CancellationToken cancellationToken = default(CancellationToken));
    }

    public abstract class IpcServiceEndpoint<TContract> : IpcServiceEndpoint
        where TContract : class
    {
        private readonly IIpcMessageSerializer _serializer;
        private readonly IpcServiceOptions _options;

        protected IpcServiceEndpoint(string name, IServiceProvider serviceProvider, IpcServiceOptions serviceOptions)
            : base(name, serviceProvider)
        {
            _serializer = serviceProvider.GetRequiredService<IIpcMessageSerializer>();
            _options = serviceOptions;
        }

        protected async Task ProcessAsync(Stream server, ILogger logger, CancellationToken cancellationToken)
        {
            using (var writer = new IpcWriter(server, _serializer, _options, leaveOpen: true))
            using (var reader = new IpcReader(server, _serializer, _options, leaveOpen: true))
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] client connected, reading request...");
                    IpcRequest request = await reader.ReadIpcRequestAsync(cancellationToken).ConfigureAwait(false);

                    cancellationToken.ThrowIfCancellationRequested();

                    logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] request received, invoking '{request.MethodName}'...");
                    IpcResponse response;
                    using (IServiceScope scope = ServiceProvider.CreateScope())
                    {
                        response = await GetReponse(request, scope);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] sending response...");
                    await writer.WriteAsync(response, cancellationToken).ConfigureAwait(false);

                    logger?.LogDebug($"[thread {Thread.CurrentThread.ManagedThreadId}] done.");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, ex.Message);
                    await writer.WriteAsync(IpcResponse.Fail($"Internal server error: {ex.Message}"), cancellationToken);
                }
            }
        }

        protected async Task<IpcResponse> GetReponse(IpcRequest request, IServiceScope scope)
        {
            object service = scope.ServiceProvider.GetService<TContract>();
            if (service == null)
            {
                return IpcResponse.Fail($"No implementation of interface '{typeof(TContract).FullName}' found.");
            }

            MethodInfo method = service.GetType().GetMethod(request.MethodName);
            if (method == null)
            {
                return IpcResponse.Fail($"Method '{request.MethodName}' not found in interface '{typeof(TContract).FullName}'.");
            }

            ParameterInfo[] paramInfos = method.GetParameters();
            if (paramInfos.Length != request.Parameters.Length)
            {
                return IpcResponse.Fail($"Parameter mismatch.");
            }
            /*
            Type[] genericArguments = method.GetGenericArguments();
            if (genericArguments.Length != request.GenericArguments.Length)
            {
                return IpcResponse.Fail($"Generic arguments mismatch.");
            }
            */
          //  object[] args = new object[paramInfos.Length];
           // for (int i = 0; i < args.Length; i++)
         //   {
           //     object origValue = request.Parameters[i];
           //     args[i] = origValue;
                /*
                Type destType = paramInfos[i].ParameterType;
                if (destType.IsGenericParameter)
                {
                    destType = request.GenericArguments[destType.GenericParameterPosition];
                }

                args[i] = destType;

                if (_converter.TryConvert(origValue, destType, out object arg))
                {
                    args[i] = arg;
                }
                else
                {
                    return IpcResponse.Fail($"Cannot convert value of parameter '{paramInfos[i].Name}' ({origValue}) from {origValue.GetType().Name} to {destType.Name}.");
                }
                */
         //   }
            
            try
            {
               // if (method.IsGenericMethod)
               // {
               //     method = method.MakeGenericMethod(request.Parameters);
               // }

                object res = method.Invoke(service, request.Parameters);

                if (res is Task)
                {
                    await (Task)res;

                    var resultProperty = res.GetType().GetProperty("Result");
                    return IpcResponse.Success(resultProperty?.GetValue(res));
                }
                else
                {
                    return IpcResponse.Success(res);
                }
            }
            catch (Exception ex)
            {
                return IpcResponse.Fail($"Internal server error: {ex.Message}");
            }
        }
    }
}
﻿using Castle.DynamicProxy;
using K2adev.IpcServiceFramework.IO;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace K2adev.IpcServiceFramework
{
    public abstract class IpcServiceClient<TInterface>
        where TInterface : class
    {
        private static readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
        private readonly IIpcMessageSerializer _serializer;
        private readonly IpcServiceOptions _options;

        protected IpcServiceClient(
            IIpcMessageSerializer serializer, IpcServiceOptions options)
        {
            _serializer = serializer;
            _options = options;
        }

        public async Task InvokeAsync(Expression<Action<TInterface>> exp,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IpcRequest request = GetRequest(exp, new MyInterceptor());
            IpcResponse response = await GetResponseAsync(request, cancellationToken);

            if (response.Succeed)
            {
                return;
            }
            else
            {
                throw new InvalidOperationException(response.Failure);
            }
        }

        public async Task<TResult> InvokeAsync<TResult>(Expression<Func<TInterface, TResult>> exp,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IpcRequest request = GetRequest(exp, new MyInterceptor<TResult>());
            IpcResponse response = await GetResponseAsync(request, cancellationToken);

            if (response.Succeed)
            {
                return (TResult)response.Data;
            }
            else
            {
                throw new InvalidOperationException(response.Failure);
            }
        }

        public async Task InvokeAsync(Expression<Func<TInterface, Task>> exp,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IpcRequest request = GetRequest(exp, new MyInterceptor<Task>());
            IpcResponse response = await GetResponseAsync(request, cancellationToken);

            if (response.Succeed)
            {
                return;
            }
            else
            {
                throw new InvalidOperationException(response.Failure);
            }
        }

        public async Task<TResult> InvokeAsync<TResult>(Expression<Func<TInterface, Task<TResult>>> exp,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IpcRequest request = GetRequest(exp, new MyInterceptor<Task<TResult>>());
            IpcResponse response = await GetResponseAsync(request, cancellationToken);

            if (response.Succeed)
            {
                return (TResult)response.Data;
            }
            else
            {
                throw new InvalidOperationException(response.Failure);
            }
        }


        private static IpcRequest GetRequest(Expression exp, MyInterceptor interceptor)
        {
            if (!(exp is LambdaExpression lamdaExp))
            {
                throw new ArgumentException("Only support lamda expresion, ex: x => x.GetData(a, b)");
            }

            if (!(lamdaExp.Body is MethodCallExpression methodCallExp))
            {
                throw new ArgumentException("Only support calling method, ex: x => x.GetData(a, b)");
            }

            TInterface proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget<TInterface>(interceptor);
            Delegate @delegate = lamdaExp.Compile();
            @delegate.DynamicInvoke(proxy);

            return new IpcRequest
            {
                MethodName = interceptor.LastInvocation.Method.Name,
                Parameters = interceptor.LastInvocation.Arguments,
            };
        }

        protected abstract Task<Stream> ConnectToServerAsync(CancellationToken cancellationToken);

        private async Task<IpcResponse> GetResponseAsync(IpcRequest request, CancellationToken cancellationToken)
        {
            using (Stream client = await ConnectToServerAsync(cancellationToken))
            using (var writer = new IpcWriter(client, _serializer, _options, leaveOpen: true))
            using (var reader = new IpcReader(client, _serializer, _options, leaveOpen: true))
            {
                // send request
                await writer.WriteAsync(request, cancellationToken).ConfigureAwait(false);

                // receive response
                return await reader.ReadIpcResponseAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private class MyInterceptor : IInterceptor
        {
            public IInvocation LastInvocation { get; private set; }

            public virtual void Intercept(IInvocation invocation)
            {
                LastInvocation = invocation;
            }
        }

        private class MyInterceptor<TResult> : MyInterceptor
        {
            public override void Intercept(IInvocation invocation)
            {
                base.Intercept(invocation);
                invocation.ReturnValue = default(TResult);
            }
        }
    }
}

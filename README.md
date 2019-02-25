# Origin
Based on [Jacques Kang's IpcServiceFramework](https://github.com/jacqueskang/IpcServiceFramework/). Partial backward compatibility.

# IpcServiceFramework

A .NET Core lightweight inter-process communication framework allowing invoking a service via named pipeline and/or TCP (in a similar way as WCF, which is currently unavailable for .NET Core). 
GZip compression and AES256 encryption are also supported. No backward compatibility with this features.

Support using primitive or complex types in service contract.
Support multi-threading on server side with configurable number of threads (named pipeline endpoint only).

[ASP.NET Core Dependency Injection framework](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) friendly.

## Usage
 1. Create an interface as service contract and package it in an assembly to be shared between server and client.
 2. Implement the service and host it in an console or web applciation.
 3. Invoke the service with framework provided proxy client.

## Downloads

IpcServiceFramework is available via NuGet:

 - [K2adev.IpcServiceFramework.Server](https://www.nuget.org/packages/K2adev.IpcServiceFramework.Server/)
 - [K2adev.IpcServiceFramework.Client](https://www.nuget.org/packages/K2adev.IpcServiceFramework.Client/)

## Quick Start:

### Step 1: Create service contract
```csharp
    public interface IComputingService
    {
        float AddFloat(float x, float y);
    }
```
_Note: This interface is ideally to be placed in a library assembly to be shared between server and client._

### Step 2: Implement the server

1. Create a console application with the following 2 NuGet packages installed:

```
> Install-Package Microsoft.Extensions.DependencyInjection
> Install-Package K2adev.IpcServiceFramework.Server
```

2. Add an class that implements the service contract

```csharp
    class ComputingService : IComputingService
    {
        public float AddFloat(float x, float y)
        {
            return x + y;
        }
    }
```

3. Configure and run the server

```csharp
    class Program
    {
        static void Main(string[] args)
        {
            // configure DI
            IServiceCollection services = ConfigureServices(new ServiceCollection());

            // build and run service host
            new IpcServiceHostBuilder(services.BuildServiceProvider())
                .AddNamedPipeEndpoint<IComputingService>(name: "endpoint1", pipeName: "pipeName")
                .AddTcpEndpoint<IComputingService>(name: "endpoint2", ipEndpoint: IPAddress.Loopback, port: 45684)
                .Build()
                .Run();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddIpc(builder =>
                {
                    builder
                        .AddNamedPipe(options =>
                        {
                            options.ThreadCount = 2;
                        })
                        .AddService<IComputingService, ComputingService>();
                });
        }
    }
```
_Note: It's possible to host IPC service in web application*_

### Step 3: Invoke the service from client process

1. Install the following package in client application:
```bash
$ dotnet add package K2adev.IpcServiceFramework.Client
```

2. Add reference to the assembly created in step 1 which contains `IComputingService` interface.

3. Invoke the server

```csharp
    IpcServiceClient<IComputingService> client = new IpcServiceClientBuilder<IComputingService>()
        .UseNamedPipe("pipeName") // or .UseTcp(IPAddress.Loopback, 45684) to invoke using TCP
        .Build();

    float result = await client.InvokeAsync(x => x.AddFloat(1.23f, 4.56f));
```

__Welcome to raise any issue or even provide any suggestion/PR to participate this project!__

## Security

You can use AES256 encryption on both TCP or PIPE implementations.
Unlike original IpcServiceFramework there is no SSL support. 

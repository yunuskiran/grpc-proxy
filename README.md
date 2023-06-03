# gRPC Proxy

**gRPC Proxy** is a library that provides a generic proxy implementation for gRPC services. It allows you to intercept and modify incoming gRPC requests and outgoing responses, enabling you to add cross-cutting concerns such as logging, authentication, authorization, and more.

## Features

- Generic proxy implementation for gRPC services
- Intercepts incoming requests and outgoing responses
- Supports modifying requests and responses through proxy dispatchers
- Provides flexibility to add cross-cutting concerns to gRPC services
- Compatible with any gRPC service definition


## Usage

To use gRPC Proxy, follow these steps:

1. Define your gRPC service interface.
2. Create a list of proxy dispatchers that implement the `IProxyDispatcher<TRequest, TResponse>` interface.
3. Instantiate the `GenericGrpcProxy<TService, TRequest, TResponse>` class.
4. Add your proxy dispatchers using the `AddDispatchers` method.
5. Use the `Dispatch` method of the proxy instance to handle incoming gRPC requests.

Here's an example of using the gRPC Proxy with a gRPC client:

```csharp
// Create a gRPC channel and client
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new Greeter.GreeterClient(channel);

// Create a list of proxy dispatchers
var dispatchers = new List<IProxyDispatcher<Method<HelloRequest, HelloReply>, HelloReply>>();
// Add your custom proxy dispatchers to the list

// Instantiate the GenericGrpcProxy
var proxy = new GenericGrpcProxy<Greeter.GreeterClient, HelloRequest, HelloReply>();

// Add the proxy dispatchers to the proxy instance
proxy.AddDispatchers(dispatchers);

// Make a gRPC request through the proxy
var request = new HelloRequest { Name = "Alice" };
var reply = await proxy.Dispatch(Method<HelloRequest, HelloReply>.Create(call => client.SayHelloAsync(call.Request, call.Headers, call.CancellationToken)), request, context);

// Process the response
Console.WriteLine(reply.Message);
```

You can have a look web api sample for Client.




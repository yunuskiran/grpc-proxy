using Grpc.Core;
using GrpcProxy;
using System;
using System.Threading.Tasks;

namespace Client.Dispachers;

public class AuthenticationDispacher<TMethod, TResponse> : IProxyDispatcher<TMethod, TResponse>
{
    public async Task<TResponse> Dispatch(TMethod method, object request, ServerCallContext context,
        Func<object, ServerCallContext, Task<TResponse>> next) =>
        //Authenticate
        await next(request, context);
}
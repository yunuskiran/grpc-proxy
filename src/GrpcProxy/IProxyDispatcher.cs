using Grpc.Core;
using System.Threading.Tasks;
using System;

namespace GrpcProxy;

public interface IProxyDispatcher<in TMethod, TResponse>
{
    Task<TResponse> Dispatch(TMethod method, object request, ServerCallContext context,
        Func<object, ServerCallContext, Task<TResponse>> next);
}
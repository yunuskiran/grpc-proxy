using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrpcProxy;

public static class MethodBuilder
{
    public static Method<TRequest, TResponse> Create<TRequest, TResponse>()
    {
        var requestMarshaller =
            new Marshaller<TRequest>(Serializer<TRequest>.Serialize, Serializer<TRequest>.Deserialize);
        var responseMarshaller =
            new Marshaller<TResponse>(Serializer<TResponse>.Serialize, Serializer<TResponse>.Deserialize);

        return new Method<TRequest, TResponse>(MethodType.Unary, requestMarshaller, responseMarshaller);
    }

    private static Method<TRequest, TResponse> Build<TRequest, TResponse>(this Method<TRequest, TResponse> method,
        IReadOnlyList<IProxyDispatcher<Method<TRequest, TResponse>, TResponse>> dispatchers)
    {
        async Task<TResponse> Handler(TRequest request, ServerCallContext context)
        {
            async Task<TResponse> CallNextDispatcher(int index)
            {
                if (index >= dispatchers.Count)
                {
                    var taskCompletionSource = new TaskCompletionSource<TResponse>();
                    try
                    {
                        await method.Handler(request, context);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }

                    return await taskCompletionSource.Task;
                }

                return await dispatchers[index]
                    .Dispatch(method, request, context, async (_, _) => await CallNextDispatcher(index + 1));
            }

            return await CallNextDispatcher(0);
        }

        method.Handler = Handler;
        return method;
    }

    public static Method<TRequest, TResponse> Build<TRequest, TResponse>(this Method<TRequest, TResponse> method,
        List<IProxyDispatcher<Method<TRequest, TResponse>, TResponse>> dispatchers, MethodType type)
    {
        method.Type = type;
        return method.Build(dispatchers);
    }
}
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace GrpcProxy;

public class Method<TRequest, TResponse>
{
    public Func<TRequest, ServerCallContext, Task<TResponse>> Handler { get; set; }
    public MethodType Type { get; set; }
    public Marshaller<TRequest> RequestMarshaller { get; }
    public Marshaller<TResponse> ResponseMarshaller { get; }

    public Method(MethodType type, Marshaller<TRequest> requestMarshaller,
        Marshaller<TResponse> responseMarshaller)
    {
        Type = type;
        RequestMarshaller = requestMarshaller;
        ResponseMarshaller = responseMarshaller;
    }
}
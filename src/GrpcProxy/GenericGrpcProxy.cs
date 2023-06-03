using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GrpcProxy;

public interface IGenericGrpcProxy<TRequest, TResponse> where TRequest : class
    where TResponse : class
{
    void AddDispatcher<TDispatcher>(TDispatcher dispatcher)
        where TDispatcher : IProxyDispatcher<Method<TRequest, TResponse>, TResponse>;

    Task<TResponse> Dispatch(string methodName, TRequest request, ServerCallContext context,
        Func<TRequest, ServerCallContext, Task<TResponse>> next);
}

public class GenericGrpcProxy<TService, TRequest, TResponse> : IGenericGrpcProxy<TRequest, TResponse>
    where TService : class
    where TRequest : class
    where TResponse : class
{
    private readonly List<IProxyDispatcher<Method<TRequest, TResponse>, TResponse>> _dispatchers;
    private readonly TService _implementation;

    public GenericGrpcProxy(IEnumerable<IProxyDispatcher<Method<TRequest, TResponse>, TResponse>> dispatchers,
        TService implementation)
    {
        _implementation = implementation;
        _dispatchers = dispatchers.ToList();
        BindService();
    }

    public void AddDispatcher<TDispatcher>(TDispatcher dispatcher)
        where TDispatcher : IProxyDispatcher<Method<TRequest, TResponse>, TResponse> => _dispatchers.Add(dispatcher);

    public async Task<TResponse> Dispatch(string methodName, TRequest request, ServerCallContext context,
        Func<TRequest, ServerCallContext, Task<TResponse>> next)
    {
        var method = FindMethod(methodName) ?? throw new ArgumentException($"Method '{methodName}' not found.");

        async Task<TResponse> CallNextDispatcher(int index)
        {
            if (index >= _dispatchers.Count)
            {
                return await next(request, context);
            }

            return await _dispatchers[index]
                .Dispatch(method, request, context, async (_, _) => await CallNextDispatcher(index + 1));
        }

        return await CallNextDispatcher(0);
    }

    private Method<TRequest, TResponse> FindMethod(string methodName)
    {
        var result = default(Method<TRequest, TResponse>);
        var methods = typeof(TService).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        var method = methods.FirstOrDefault(_ =>
            _.DeclaringType?.Namespace != null && _.DeclaringType != null && _.Name == methodName &&
            !_.DeclaringType.Namespace.StartsWith("System"));
        if (method != null)
        {
            result = MethodBuilder.Create<TRequest, TResponse>()
                .Build(_dispatchers, MethodType.Unary);
        }

        return result;
    }

    private void BindService()
    {
        var serviceDefinition = ServerServiceDefinition.CreateBuilder();
        var methods = typeof(TService).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        var processedMethodNames = new HashSet<string>();
        foreach (var methodInfo in methods)
        {
            if (methodInfo.DeclaringType?.Namespace != null && methodInfo.DeclaringType != null &&
                methodInfo.DeclaringType.Namespace.StartsWith("System"))
                continue;
            var methodName = methodInfo.Name;
            if (processedMethodNames.Contains(methodName))
            {
                continue;
            }

            processedMethodNames.Add(methodName);

            var method = MethodBuilder.Create<TRequest, TResponse>()
                .Build(_dispatchers, MethodType.Unary);
            var unaryHandler = new UnaryServerMethod<TRequest, TResponse>(
                async (request, context) => await method.Handler(request, context));
            var grpcMethod = new Grpc.Core.Method<TRequest, TResponse>(method.Type, methodName, methodName,
                method.RequestMarshaller, method.ResponseMarshaller);
            serviceDefinition.AddMethod(grpcMethod, unaryHandler);
        }

        serviceDefinition.Build();
    }
}
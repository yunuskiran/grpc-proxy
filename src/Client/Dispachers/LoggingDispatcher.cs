using Grpc.Core;
using GrpcProxy;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Client.Dispachers;

public class LoggingDispatcher<TMethod, TResponse> : IProxyDispatcher<TMethod, TResponse>
{
    private readonly ILogger _logger;

    public LoggingDispatcher(ILogger logger) => _logger = logger;

    public async Task<TResponse> Dispatch(TMethod method, object request, ServerCallContext context,
        Func<object, ServerCallContext, Task<TResponse>> next)
    {
        var methodName = GetMethodName(method);
        _logger.Information($"Before calling method: {methodName}");
        var response = await next(request, context);
        _logger.Information($"After calling method: {methodName}");
        return response;
    }

    private static string GetMethodName(TMethod method)
    {
        var methodInfo = method.GetType().GetProperty("Name");
        return methodInfo?.GetValue(method)?.ToString();
    }
}
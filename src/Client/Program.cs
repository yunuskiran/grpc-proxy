using Client;
using Grpc.Net.Client;
using GrpcProxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((_, config) => { config.WriteTo.Console(); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Client", Version = "v1" }); });

builder.Services.AddSingleton(_ =>
{
    var channel = GrpcChannel.ForAddress("https://localhost:7052");
    return new Greeter.GreeterClient(channel);
});

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo(typeof(IProxyDispatcher<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddScoped(typeof(IGenericGrpcProxy<,>), typeof(GenericGrpcProxy<,,>));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/test", async (IGenericGrpcProxy<HelloRequest, HelloReply> proxy, HttpContext context) =>
    {
        var request = new HelloRequest { Name = "GreeterClient" };

        var response = await proxy.Dispatch("SayHelloAsync", request, null, async (req, _) =>
        {
            var client = context.RequestServices.GetRequiredService<Greeter.GreeterClient>();
            return await client.SayHelloAsync(req);
        });

        return response;
    })
    .WithName("test")
    .WithOpenApi();

app.Run();
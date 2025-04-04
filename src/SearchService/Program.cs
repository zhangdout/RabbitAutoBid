using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMQ:Username", "guest")!);
            h.Password(builder.Configuration.GetValue("RabbitMQ:Password", "guest")!);
        });

        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            //如果消费者处理消息时发生异常，系统会 最多重试 5 次。
            //每次重试之间 等待 5 秒。
            e.UseMessageRetry(r => r.Interval(5, 5));
            //让 search-auction-created 队列的消息被 AuctionCreatedConsumer 处理。
            //如果消费者（Consumer）处理消息时发生异常，并且 重试次数用完，该消息不会丢失，而是被发送到 faultQueue（通常叫 error 队列）。
            //可以理解为 RabbitMQ 里的 "死信队列"（Dead Letter Queue, DLQ），但它是 MassTransit 自动管理的错误队列。
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

// 这段代码在 应用启动时 运行 一次，但不会立刻执行，只是注册回调方法。
// 当 ASP.NET Core 服务器完全启动 时（即 app.Run() 之后），才会执行 ApplicationStarted 回调。
//为什么把回调函数放在这里：
// 当auction service下线时，如果重启search service，要search service保证app先启动，search service可以接收请求。否则search service在启动前要一直等待auction service返回数据，无法启动。
app.Lifetime.ApplicationStarted.Register(async () =>
{
    await Policy.Handle<TimeoutException>()
        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
        .ExecuteAndCaptureAsync(async () => await DbInitializer.InitDb(app));
});

// app.Run() 启动 Kestrel 服务器，监听 HTTP 请求。
//当 app.Run() 成功启动服务器后，会触发 ApplicationStarted 事件。
//此时，app.Lifetime.ApplicationStarted.Register(...) 注册的回调函数被执行。
app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() //IAsyncPolicy<HttpResponseMessage> 是 Polly 提供的接口，表示这个策略适用于返回 HttpResponseMessage 的操作
    => HttpPolicyExtensions
        .HandleTransientHttpError()  //短暂性错误（Transient Errors） 是短时间后可能自动恢复的网络或服务器错误，如 超时（408）、服务器错误（500）、网关错误（502、504）等。
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3)); //指定 每次重试的间隔为 3 秒

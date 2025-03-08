using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/*
注册 MVC 控制器（Controllers）
允许 ASP.NET Core 处理 HTTP 请求
默认作用域：Singleton
📌 为什么是 Singleton？

AddControllers() 注册的是 Controller 相关的服务，而不是 Controller 本身
Controller 是 Transient，每次请求创建新的实例
但 AddControllers() 里注册的 MVC 组件（如 ModelBinder、Routing）是 Singleton
*/
builder.Services.AddControllers();

/*
DbContext 是数据库连接的核心对象，必须在请求结束后释放
每个 HTTP 请求创建一个 DbContext 实例
请求结束后，DbContext 会自动释放，避免数据库连接泄漏
避免不同请求共享 DbContext，防止数据不一致
确保事务在 HTTP 请求结束时提交或回滚
*/

/*
每个 HTTP 请求都会创建新的 DbContext，然后在请求结束时销毁，但这实际上 并不会带来严重的性能问题，因为：
DbContext 是一个轻量级对象，不会持有大量内存或连接。
DbContext 使用数据库连接池，所以即使 DbContext 本身被销毁，底层数据库连接不会每次都关闭，而是会回到连接池中复用。
DbContext 生命周期短，减少了内存泄漏风险和数据不一致问题。
*/
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    //在 ASP.NET Core 中，Program.cs 里的 builder.Configuration 默认已经包含 IConfiguration，所以不需要显式注入。
    /*
    builder.Configuration 是 ASP.NET Core 默认创建的 IConfiguration 实例，它包含：
    appsettings.json
    appsettings.{Environment}.json
    环境变量
    命令行参数
    */
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

/*
注册 AutoMapper，用于对象映射
默认作用域：Singleton
📌 为什么是 Singleton？

AutoMapper 配置只需要加载一次
所有请求共享同一个 AutoMapper 配置
映射规则不会改变，适合 Singleton
*/
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(x =>
{
    // masstransit每10秒钟从outbox查看一次是否有未发送的消息
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);

        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq( (context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();



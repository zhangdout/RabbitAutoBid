using System;
using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AuctionService.IntegrationTests.Fixtures;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{

    private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    public async Task InitializeAsync()
    {
        //启动一个包含测试数据库的容器
        await _postgreSqlContainer.StartAsync();
    }

    //自定义 Web API 的测试环境
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services => 
        {
            services.RemoveDbContext<AuctionDbContext>();

            //重新添加 DbContext（连接测试数据库）
            //这样，每次运行测试时，都会启动一个新的 PostgreSQL 容器，确保测试环境干净、可复现。
            services.AddDbContext<AuctionDbContext>(options => 
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString()); //动态获取测试数据库的连接字符串。
            });

            //创建一个内存中的 MassTransit 测试环境，用于测试事件发布和消费，而不需要真正的 RabbitMQ/Kafka 服务器。
            services.AddMassTransitTestHarness();

            services.EnsureCreated<AuctionDbContext>();

        });
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
}

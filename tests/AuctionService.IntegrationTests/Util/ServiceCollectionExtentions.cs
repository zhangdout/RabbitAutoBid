using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
    {
        //查找并移除 生产环境中的 AuctionDbContext 依赖（即数据库配置）。
        //AuctionDbContext 可能使用了AuctionService配置的 SQL Server/PostgreSQL 配置，但我们想要替换成测试数据库。
        var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

        if (descriptor != null) services.Remove(descriptor);
    }

    public static void EnsureCreated<T>(this IServiceCollection services)
    {
        /*
            作用
            手动获取 DI 容器的 AuctionDbContext，并执行 Database.Migrate()。
            Migrate() 确保数据库根据最新的 EF Core 迁移文件创建或更新。
            📌 为什么这样做？

            由于 AuctionDbContext 是动态替换的，ASP.NET Core 默认不会自动执行 Migrate()。
            这样保证数据库 schema 与代码同步，避免测试时出现 Table not found 之类的错误。
            */
        var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AuctionDbContext>();
        db.Database.Migrate();
        DbHelper.InitDbForTests(db);
    }
}
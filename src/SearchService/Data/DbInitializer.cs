using System;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    // 一个静态方法，用于在program.cs中，给web app初始化mongodb并且seed data。
    public static async Task InitDb(WebApplication app)
    {
        // 初始化mongodb

        await DB.InitAsync("SearchDB", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        // DB.Index<T>() 是 MongoDB.Entities 框架提供的 索引创建方法，用于在 MongoDB 的 Item 集合上创建全文搜索索引
        // 这里的 Item 是 MongoDB 的集合（Collection），即 MongoDB 数据库中的一个表
        // 这表示我们要为 Item 集合创建索引。
        /*
        .Key(x => x.Make, KeyType.Text) 表示为 Make 字段创建文本索引。
        KeyType.Text 表示 全文搜索索引（Text Index）。支持全文搜索。全文搜索优于传统搜索，可以匹配同义词、支持多字段等。
        多个字段可以创建 Text 索引，以便进行全文搜索。
        */
        // 这段代码它在 Item 集合（Collection） 上创建 文本索引，使得 Make、Model 和 Color 可以被 MongoDB 进行全文搜索。

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();

        /*
        // 从预定义的文件中获取seed data
        if (count == 0)
        {
            Console.WriteLine("No data - will attempt to seed.");
            var itemData = await File.ReadAllTextAsync("Data/auctions.json");

            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

            var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

            await DB.SaveAsync(items); 
        }
        */

        //distributed monolith
        //不是微服务架构
        // 发送http请求到Auction Service获取seed Data

        /*
        app.Services.CreateScope() 创建一个 IServiceScope，用于获取 Scoped 或 Transient 类型的依赖
        using var 让 scope 在代码块结束时自动释放
        确保获取的 HttpClient 只在当前作用域内有效
        📌 为什么需要 scope？
        在 Program.cs 里 app.Services 默认是 Singleton，但 HttpClient 不能是 Singleton
        AuctionSvcHttpClient 是 Transient 或 Scoped，所以需要 CreateScope() 以获取实例
        ✅ 这样不会影响应用程序的主服务生命周期
        */

        List<Item> items = new List<Item>();
        /*
        在 ASP.NET Core 依赖注入（Dependency Injection, DI） 中，服务的生命周期主要有三种：
        Singleton（单例） → 应用程序生命周期内共享同一个实例
        Scoped（作用域） → 每个 HTTP 请求创建一个新实例
        Transient（瞬态） → 每次获取都会创建一个新实例
        */
        //确保 AuctionSvcHttpClient 由 DI 提供，而不是手动创建
        using (var scope = app.Services.CreateScope())
        {
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
            items = await httpClient.GetItemsForSearchDb();
        }

        Console.WriteLine(items.Count + " returned from auction service");

        if (items.Count > 0) await DB.SaveAsync(items);
    } 
}

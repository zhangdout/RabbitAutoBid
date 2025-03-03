using System;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    //HttpClient 是 .NET 用于发送 HTTP 请求和接收 HTTP 响应的类
    //IConfiguration 用于读取 appsettings.json 或环境变量中的配置
    //在 ASP.NET Core 中，Program.cs 里的 builder.Configuration 默认已经包含 IConfiguration，所以不需要显式注入。
    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    /*
    先查询 MongoDB，获取最新 UpdatedAt
    调用 AuctionService，请求 UpdatedAt 时间之后的新数据
    只同步新增/修改的数据，减少数据传输
    */
    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync(); //获取 第 1 条数据

        return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] 
            + "/api/auctions?date=" + lastUpdated);
    }
}

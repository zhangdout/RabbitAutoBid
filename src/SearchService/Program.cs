using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

// 初始化mongodb

await DB.InitAsync("SearchDB", MongoClientSettings
    .FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));

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

app.Run();

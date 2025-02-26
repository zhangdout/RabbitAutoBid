using System;
using MongoDB.Entities;

namespace SearchService.Models;

/*
Entity 是 MongoDB.Entities 库中提供的一个基类，所有要存入 MongoDB 的实体（表）都应该继承它。它的作用类似于其他 ORM 框架（如 Entity Framework）的 BaseEntity，通常包含：
ID 生成：提供 _id（MongoDB 默认主键）的支持。
数据库集合管理：自动将该类映射到 MongoDB 的集合（Collection）。
CRUD 支持：继承 Entity 后，可以使用 SaveAsync()、DeleteAsync() 等便捷方法进行数据库操作。
*/

public class Item : Entity
{
    public int ReservePrice { get; set; }
    public string Seller { get; set; }
    public string Winner { get; set; }
    public int SoldAmount {get;set;}
    public int CurrentHighBid {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime UpdatedAt {get;set;}
    public DateTime AuctionEnd {get;set;}
    public string Status {get;set;}
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public int Mileage { get; set; }
    public string ImageUrl { get; set; }
}

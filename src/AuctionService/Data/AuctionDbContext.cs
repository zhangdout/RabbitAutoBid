using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AuctionService.Entities;
using MassTransit;

namespace AuctionService.Data;

// DbContext 是 EF Core 提供的 核心类，用于管理数据库操作（如增删改查）。
// DbContextOptions 是 EF Core 用来配置 DbContext 的 配置类。它不执行数据库操作，只是 提供数据库的连接方式、提供程序（SQL Server, SQLite, PostgreSQL 等）等配置信息。
// DbContext 本身不存储数据库连接信息，它只会执行操作（如 Add()、Remove()、SaveChanges()）。DbContextOptions 提供数据库连接和行为配置，DbContext 必须通过 DbContextOptions 获取这些配置。
public class AuctionDbContext : DbContext
{
    // DbContext 没有无参构造函数，所以 AuctionDbContext 必须手动调用 base(options) 来初始化 DbContext。
    public AuctionDbContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<Auction> Auctions {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}



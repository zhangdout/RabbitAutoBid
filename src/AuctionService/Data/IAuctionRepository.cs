using System;
using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Data;

public interface IAuctionRepository
{
    //创建 Repository（仓储） 主要用于模拟数据访问，使测试能够独立于数据库运行，同时确保测试的可控性和稳定性。
    Task<List<AuctionDto>> GetAuctionsAsync(string date);
    Task<AuctionDto> GetAuctionByIdAsync(Guid id);
    Task<Auction> GetAuctionEntityById(Guid id);
    void AddAuction(Auction auction);
    void RemoveAuction(Auction auction);
    Task<bool> SaveChangesAsync();
}

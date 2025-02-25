using System;
using AuctionService.Data;
using AuctionService.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions()
    {
        // 从数据库中的拍卖表中获取数据
        var auctions = await _context.Auctions //auction的类型是List<Auction>
            .Include(x => x.Item) //Include方法预加载(eager loading)关联的Auction.Item数据到内存中。后续使用不需要再执行查询。            
            .OrderBy(x => x.Item.Make)
            .ToListAsync(); //如果你使用了await，就必须调用返回Task类型的方法，例如ToListAsync()

        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }
}

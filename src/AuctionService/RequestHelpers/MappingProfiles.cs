using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers;
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        /* CreateMap<Auction, AuctionDto>()：告诉 AutoMapper 映射 Auction → AuctionDto。
            .IncludeMembers(x => x.Item)：
            让 AutoMapper 在 Auction 里找不到的字段时，去 Item 里找！
            确保 Item 里的 Make、Model、Year 等字段正确映射到 AuctionDto。*/
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        /* IncludeMembers(x => x.Item) 只是告诉 AutoMapper AuctionDto 里的一些字段来自 Item。
            但是 AutoMapper 本身并不知道如何从 Item 里获取这些字段，所以你必须显式定义 Item → AuctionDto 的映射。*/
        CreateMap<Item, AuctionDto>();
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(d => d.Item, o => o.MapFrom(s => s));
        CreateMap<CreateAuctionDto, Item>();
    }
}
using System;
using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);
        //_auctionRepo 和 _publishEndpoint 是 Moq 创建的 Mock<T> 对象，而 Object 属性才是获取 Mock<T> 的真实模拟实例
        _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
            }
        };
    }

    /*
    该测试问题： 只是验证 Mock 行为，没有实际测试业务逻辑。
    ✅ 改进方案：
    测试异常处理（确保 Controller 在数据库异常时返回 500）。
    测试边界情况（空数据、null 处理）。
    让测试更贴近实际业务逻辑，而不是 Mock 行为。
    */
    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auctions()
    {
        // arrange
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
            //设定 Mock：当 `GetAuctionsAsync(null)` 被调用时，返回上面设定的包含10个随机AuctionDto的 `auctions`
        _auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);

        // act
        var result = await _controller.GetAuctions(null);

        // assert
        Assert.Equal(10, result.Value?.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        // arrange
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);

        // act
        var result = await _controller.GetAuctionById(auction.Id);

        // assert
        Assert.Equal(auction.Make, result.Value?.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithInValidGuid_ReturnsNotFound()
    {
        // arrange
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        // act
        var result = await _controller.GetAuctionById(Guid.NewGuid());

        // assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithInValidCreateAuctionDto_ReturnsCreatedAtAction()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.CreateAuction(auction);
        var createdResult = result.Result as CreatedAtActionResult;

        // assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDto>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // act
        var result = await _controller.CreateAuction(auction);

        // assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        // arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "test";
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateDto);

        // assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "not-test";
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateDto);

        // assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        var updateDto = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.UpdateAuction(auction.Id, updateDto);

        // assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "test";

        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _controller.DeleteAuction(auction.Id);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "test";

        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        var result = await _controller.DeleteAuction(auction.Id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "not-test";

        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _controller.DeleteAuction(auction.Id);

        Assert.IsType<ForbidResult>(result);
    }


}

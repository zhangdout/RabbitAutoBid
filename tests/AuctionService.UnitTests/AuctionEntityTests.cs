using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    //在 xUnit中，[Fact] 是一个测试方法的注解（attribute），用于标识无参数的单元测试方法。
    //告诉 xUnit 这个方法是一个测试，在运行测试时会被自动执行。不接受参数，适用于简单、固定输入的测试。由 xUnit 运行时自动发现和执行。
    [Fact]
    public void HasReservePrice_ReservePriceGtZero_True()
    {
        //在**单元测试（Unit Test）**中，Arrange-Act-Assert（AAA 模式）是一种标准的测试结构，用于组织和编写测试代码，使其更具可读性和可维护性。
        // Arrange（安排）：创建被测对象，设置输入数据
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReservePrice = 10
        };
        
        // Act（执行）：调用被测方法
        var result = auction.HasReservePrice();

        // Assert（断言）：验证结果是否正确
        Assert.True(result);
    }

    [Fact]
    public void HasReservePrice_ReservePriceIsZero_False()
    {
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReservePrice = 0
        };
        
        var result = auction.HasReservePrice();

        Assert.False(result);
    }
}
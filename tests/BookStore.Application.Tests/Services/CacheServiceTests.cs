using BookStore.Application.Constants;
using BookStore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;

namespace BookStore.Application.Tests.Services;

public class CacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _sut = new CacheService(_memoryCache);
    }

    [Fact]
    public void Get_ReturnsDefault_WhenKeyDoesNotExist()
    {
        // Act
        var result = _sut.Get<string>("nonexistent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        _sut.Set(key, value);

        // Act
        var result = _sut.Get<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Set_StoresValue_WithDefaultExpiration()
    {
        // Arrange
        var key = "test-key";
        var value = new TestData { Id = 1, Name = "Test" };

        // Act
        _sut.Set(key, value);

        // Assert
        var result = _sut.Get<TestData>(key);
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public void Set_StoresValue_WithCustomExpiration()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMinutes(10);

        // Act
        _sut.Set(key, value, expiration);

        // Assert
        var result = _sut.Get<string>(key);
        result.Should().Be(value);
    }

    [Fact]
    public void Set_OverwritesPreviousValue_WhenKeyExists()
    {
        // Arrange
        var key = "test-key";
        _sut.Set(key, "original-value");

        // Act
        _sut.Set(key, "new-value");

        // Assert
        var result = _sut.Get<string>(key);
        result.Should().Be("new-value");
    }

    [Fact]
    public void InvalidateBooksCache_RemovesBooksKey()
    {
        // Arrange
        _sut.Set(CacheKeys.AllBooks, new List<string> { "Book 1", "Book 2" });
        _sut.Get<List<string>>(CacheKeys.AllBooks).Should().NotBeNull();

        // Act
        _sut.InvalidateBooksCache();

        // Assert
        _sut.Get<List<string>>(CacheKeys.AllBooks).Should().BeNull();
    }

    [Fact]
    public void InvalidateOrdersCache_RemovesAllOrderKeys()
    {
        // Arrange
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();
        
        _sut.Set(CacheKeys.AllOrders, new List<string> { "Order 1", "Order 2" });
        _sut.Set(CacheKeys.OrderBooks(orderId1), new List<string> { "Book 1" });
        _sut.Set(CacheKeys.OrderBooks(orderId2), new List<string> { "Book 2" });

        // Act
        _sut.InvalidateOrdersCache();

        // Assert
        _sut.Get<List<string>>(CacheKeys.AllOrders).Should().BeNull();
        _sut.Get<List<string>>(CacheKeys.OrderBooks(orderId1)).Should().BeNull();
        _sut.Get<List<string>>(CacheKeys.OrderBooks(orderId2)).Should().BeNull();
    }

    [Fact]
    public void InvalidateOrdersCache_DoesNotAffectBooksCache()
    {
        // Arrange
        _sut.Set(CacheKeys.AllBooks, new List<string> { "Book 1", "Book 2" });
        _sut.Set(CacheKeys.AllOrders, new List<string> { "Order 1" });

        // Act
        _sut.InvalidateOrdersCache();

        // Assert
        _sut.Get<List<string>>(CacheKeys.AllBooks).Should().NotBeNull();
    }

    [Fact]
    public void InvalidateBooksCache_DoesNotAffectOrdersCache()
    {
        // Arrange
        _sut.Set(CacheKeys.AllBooks, new List<string> { "Book 1" });
        _sut.Set(CacheKeys.AllOrders, new List<string> { "Order 1" });

        // Act
        _sut.InvalidateBooksCache();

        // Assert
        _sut.Get<List<string>>(CacheKeys.AllOrders).Should().NotBeNull();
    }

    [Fact]
    public void Get_ReturnsCorrectType_ForComplexObjects()
    {
        // Arrange
        var key = "complex-key";
        var value = new List<TestData>
        {
            new() { Id = 1, Name = "Test 1" },
            new() { Id = 2, Name = "Test 2" }
        };
        _sut.Set(key, value);

        // Act
        var result = _sut.Get<List<TestData>>(key);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Id.Should().Be(1);
        result[1].Name.Should().Be("Test 2");
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

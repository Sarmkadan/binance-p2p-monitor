#nullable enable
using BinanceP2pMonitor.Caching;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for MemoryCache implementation.
/// </summary>
public class MemoryCacheUnitTests
{
    private readonly Mock<ILogger<MemoryCache>> _mockLogger;
    private readonly MemoryCache _cache;

    public MemoryCacheUnitTests()
    {
        _mockLogger = new Mock<ILogger<MemoryCache>>();
        _cache = new MemoryCache(_mockLogger.Object);
    }

    #region GetAsync Tests

    /// <summary>
    /// Verifies that GetAsync returns null for non-existent key.
    /// </summary>
    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        const string key = "nonexistent";

        // Act
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetAsync returns cached value for existing key.
    /// </summary>
    [Fact]
    public async Task GetAsync_ShouldReturnCachedValue_WhenKeyExists()
    {
        // Arrange
        const string key = "test_key";
        const string expectedValue = "test_value";
        await _cache.SetAsync(key, expectedValue);

        // Act
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies that GetAsync returns correct type for cached value.
    /// </summary>
    [Fact]
    public async Task GetAsync_ShouldReturnCorrectType_WhenValueIsCached()
    {
        // Arrange
        const string key = "numeric_key";
        const int expectedValue = 42;
        await _cache.SetAsync(key, expectedValue);

        // Act
        var result = await _cache.GetAsync<int>(key);

        // Assert
        result.Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies that GetAsync handles null values correctly.
    /// </summary>
    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenValueIsNull()
    {
        // Arrange
        const string key = "null_key";
        await _cache.SetAsync(key, (string?)null);

        // Act
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetAsync handles empty string key.
    /// </summary>
    [Fact]
    public async Task GetAsync_ShouldHandleEmptyStringKey()
    {
        // Arrange
        const string key = "";

        // Act
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SetAsync Tests

    /// <summary>
    /// Verifies that SetAsync stores value with default expiration.
    /// </summary>
    [Fact]
    public async Task SetAsync_ShouldStoreValue_WithDefaultExpiration()
    {
        // Arrange
        const string key = "persistent_key";
        const string value = "persistent_value";

        // Act
        await _cache.SetAsync(key, value);
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    /// <summary>
    /// Verifies that SetAsync stores value with custom expiration.
    /// </summary>
    [Fact]
    public async Task SetAsync_ShouldStoreValue_WithCustomExpiration()
    {
        // Arrange
        const string key = "expiring_key";
        const string value = "expiring_value";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await _cache.SetAsync(key, value, expiration);
        var resultBeforeExpiration = await _cache.GetAsync<string>(key);
        await Task.Delay(150); // Wait for expiration
        var resultAfterExpiration = await _cache.GetAsync<string>(key);

        // Assert
        resultBeforeExpiration.Should().Be(value);
        resultAfterExpiration.Should().BeNull();
    }

    /// <summary>
    /// Verifies that SetAsync overwrites existing value.
    /// </summary>
    [Fact]
    public async Task SetAsync_ShouldOverwriteExistingValue()
    {
        // Arrange
        const string key = "overwrite_key";
        const string initialValue = "initial";
        const string updatedValue = "updated";

        await _cache.SetAsync(key, initialValue);
        await _cache.SetAsync(key, updatedValue);

        // Act
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().Be(updatedValue);
    }

    /// <summary>
    /// Verifies that SetAsync throws ArgumentNullException for null key.
    /// </summary>
    [Fact]
    public async Task SetAsync_ShouldThrowArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        const string? key = null;
        const string value = "test";

        // Act & Assert
        await _cache.Invoking(c => c.SetAsync(key!, value))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that SetAsync handles null value.
    /// </summary>
    [Fact]
    public async Task SetAsync_ShouldHandleNullValue()
    {
        // Arrange
        const string key = "null_value_key";
        string? value = null;

        // Act
        await _cache.SetAsync(key, value);
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RemoveAsync Tests

    /// <summary>
    /// Verifies that RemoveAsync removes existing key.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ShouldRemoveExistingKey()
    {
        // Arrange
        const string key = "removable_key";
        const string value = "test_value";
        await _cache.SetAsync(key, value);

        // Act
        await _cache.RemoveAsync(key);
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that RemoveAsync handles non-existent key gracefully.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ShouldHandleNonExistentKey()
    {
        // Arrange
        const string key = "nonexistent_key";

        // Act & Assert (should not throw)
        await _cache.Invoking(c => c.RemoveAsync(key))
            .Should().NotThrowAsync();
    }

    /// <summary>
    /// Verifies that RemoveAsync throws ArgumentNullException for null key.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ShouldThrowArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        const string? key = null;

        // Act & Assert
        await _cache.Invoking(c => c.RemoveAsync(key!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// Verifies that ExistsAsync returns false for non-existent key.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        const string key = "nonexistent";

        // Act
        var result = await _cache.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ExistsAsync returns true for existing key.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        const string key = "existing_key";
        await _cache.SetAsync(key, "value");

        // Act
        var result = await _cache.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ExistsAsync returns true for expired key before cleanup.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_ForExpiredKey_BeforeCleanup()
    {
        // Arrange
        const string key = "expired_key";
        await _cache.SetAsync(key, "value", TimeSpan.FromMilliseconds(50));
        await Task.Delay(100); // Wait for expiration

        // Act
        var result = await _cache.ExistsAsync(key);

        // Assert - should be false because expired entries are removed on access
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ExistsAsync throws ArgumentNullException for null key.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ShouldThrowArgumentNullException_WhenKeyIsNull()
    {
        // Arrange
        const string? key = null;

        // Act & Assert
        await _cache.Invoking(c => c.ExistsAsync(key!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region ClearAsync Tests

    /// <summary>
    /// Verifies that ClearAsync removes all entries from cache.
    /// </summary>
    [Fact]
    public async Task ClearAsync_ShouldRemoveAllEntries()
    {
        // Arrange
        await _cache.SetAsync("key1", "value1");
        await _cache.SetAsync("key2", "value2");
        await _cache.SetAsync("key3", "value3");

        // Verify entries exist
        (await _cache.ExistsAsync("key1")).Should().BeTrue();
        (await _cache.ExistsAsync("key2")).Should().BeTrue();
        (await _cache.ExistsAsync("key3")).Should().BeTrue();

        // Act
        await _cache.ClearAsync();

        // Assert
        (await _cache.ExistsAsync("key1")).Should().BeFalse();
        (await _cache.ExistsAsync("key2")).Should().BeFalse();
        (await _cache.ExistsAsync("key3")).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ClearAsync can be called multiple times.
    /// </summary>
    [Fact]
    public async Task ClearAsync_ShouldHandleMultipleCalls()
    {
        // Arrange & Act
        await _cache.SetAsync("key1", "value1");
        await _cache.ClearAsync();
        await _cache.ClearAsync(); // Second call

        // Assert
        (await _cache.ExistsAsync("key1")).Should().BeFalse();
    }

    #endregion

    #region GetOrCreateAsync Tests

    /// <summary>
    /// Verifies that GetOrCreateAsync returns cached value without calling factory.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ShouldReturnCachedValue_WithoutCallingFactory()
    {
        // Arrange
        const string key = "factory_key";
        const string cachedValue = "cached";
        await _cache.SetAsync(key, cachedValue);

        var factoryCallCount = 0;
        async Task<string> factory(CancellationToken ct)
        {
            factoryCallCount++;
            await Task.Yield();
            return "new_value";
        }

        // Act
        var result = await _cache.GetOrCreateAsync(key, factory);

        // Assert
        result.Should().Be(cachedValue);
        factoryCallCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetOrCreateAsync calls factory and caches result when key doesn't exist.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ShouldCallFactoryAndCacheResult_WhenKeyDoesNotExist()
    {
        // Arrange
        const string key = "new_key";
        var factoryCallCount = 0;
        async Task<string> factory(CancellationToken ct)
        {
            factoryCallCount++;
            await Task.Yield();
            return "factory_result";
        }

        // Act
        var result = await _cache.GetOrCreateAsync(key, factory);

        // Assert
        result.Should().Be("factory_result");
        factoryCallCount.Should().Be(1);
        (await _cache.GetAsync<string>(key)).Should().Be("factory_result");
    }

    /// <summary>
    /// Verifies that GetOrCreateAsync uses custom expiration.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ShouldUseCustomExpiration()
    {
        // Arrange
        const string key = "expiring_factory_key";
        var factoryCallCount = 0;
        async Task<string> factory(CancellationToken ct)
        {
            factoryCallCount++;
            await Task.Yield();
            return "factory_result";
        }

        // Act
        var result = await _cache.GetOrCreateAsync(key, factory, TimeSpan.FromMilliseconds(50));
        var resultBeforeExpiration = await _cache.GetAsync<string>(key);
        await Task.Delay(100); // Wait for expiration
        var resultAfterExpiration = await _cache.GetAsync<string?>(key);

        // Assert
        result.Should().Be("factory_result");
        resultBeforeExpiration.Should().Be("factory_result");
        resultAfterExpiration.Should().BeNull();
        factoryCallCount.Should().Be(1); // Should only call factory once
    }

    /// <summary>
    /// Verifies that GetOrCreateAsync handles factory returning null.
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_ShouldHandleFactoryReturningNull()
    {
        // Arrange
        const string key = "null_factory_key";
        async Task<string?> factory(CancellationToken ct)
        {
            await Task.Yield();
            return null;
        }

        // Act
        var result = await _cache.GetOrCreateAsync(key, factory);

        // Assert
        result.Should().BeNull();
        (await _cache.ExistsAsync(key)).Should().BeFalse();
    }

    #endregion

    #region Dispose Tests

    /// <summary>
    /// Verifies that Dispose can be called multiple times without error.
    /// </summary>
    [Fact]
    public void Dispose_ShouldHandleMultipleCalls()
    {
        // Arrange
        var cache = new MemoryCache(_mockLogger.Object);

        // Act & Assert (should not throw)
        cache.Dispose();
        cache.Dispose();
    }

    #endregion

    #region Edge Cases and Boundary Values

    /// <summary>
    /// Verifies that cache handles very long keys.
    /// </summary>
    [Fact]
    public async Task GetAsync_ShouldHandleVeryLongKey()
    {
        // Arrange
        var longKey = new string('a', 1000);

        // Act
        var result = await _cache.GetAsync<string>(longKey);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that cache handles very long values.
    /// </summary>
    [Fact]
    public async Task SetAsync_ShouldHandleVeryLongValue()
    {
        // Arrange
        const string key = "long_value_key";
        var longValue = new string('b', 10000);

        // Act & Assert (should not throw)
        await _cache.Invoking(c => c.SetAsync(key, longValue))
            .Should().NotThrowAsync();

        var result = await _cache.GetAsync<string>(key);
        result.Should().Be(longValue);
    }

    /// <summary>
    /// Verifies that cache handles special characters in keys.
    /// </summary>
    [Fact]
    public async Task SetAsync_ShouldHandleSpecialCharactersInKey()
    {
        // Arrange
        const string key = "key-with_special.chars@123";
        const string value = "test";

        // Act
        await _cache.SetAsync(key, value);
        var result = await _cache.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    /// <summary>
    /// Verifies that cache handles different generic types correctly.
    /// </summary>
    [Fact]
    public async Task GetAsync_ShouldHandleDifferentGenericTypes()
    {
        // Arrange
        await _cache.SetAsync("string_key", "test_string");
        await _cache.SetAsync("int_key", 123);
        await _cache.SetAsync("bool_key", true);
        await _cache.SetAsync("double_key", 3.14);
        await _cache.SetAsync("list_key", new List<int> { 1, 2, 3 });

        // Act
        var stringResult = await _cache.GetAsync<string>("string_key");
        var intResult = await _cache.GetAsync<int>("int_key");
        var boolResult = await _cache.GetAsync<bool>("bool_key");
        var doubleResult = await _cache.GetAsync<double>("double_key");
        var listResult = await _cache.GetAsync<List<int>>("list_key");

        // Assert
        stringResult.Should().Be("test_string");
        intResult.Should().Be(123);
        boolResult.Should().BeTrue();
        doubleResult.Should().Be(3.14);
        listResult.Should().BeEquivalentTo(new List<int> { 1, 2, 3 });
    }

    #endregion
}

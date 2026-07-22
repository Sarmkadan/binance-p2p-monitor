using BinanceP2pMonitor.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class PerformanceMetricsExtensionsTests
{
    private readonly PerformanceMetrics _metrics = new();

    [Fact]
    public void GetSuccessRate_WithValidOperation_ReturnsCorrectSuccessRate()
    {
        // Arrange
        const string operationName = "test_operation";
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: true);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(200), success: true);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(150), success: false);

        // Act
        var successRate = _metrics.GetSuccessRate(operationName);

        // Assert
        successRate.Should().BeApproximately(66.66666666666666, 0.0000000001);
    }

    [Fact]
    public void GetSuccessRate_WithAllSuccesses_Returns100()
    {
        // Arrange
        const string operationName = "success_only";
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: true);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(200), success: true);

        // Act
        var successRate = _metrics.GetSuccessRate(operationName);

        // Assert
        successRate.Should().Be(100);
    }

    [Fact]
    public void GetSuccessRate_WithAllFailures_Returns0()
    {
        // Arrange
        const string operationName = "failures_only";
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: false);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(200), success: false);

        // Act
        var successRate = _metrics.GetSuccessRate(operationName);

        // Assert
        successRate.Should().Be(0);
    }

    [Fact]
    public void GetSuccessRate_WithNoOperations_Returns0()
    {
        // Arrange
        const string operationName = "no_ops";

        // Act
        var successRate = _metrics.GetSuccessRate(operationName);

        // Assert
        successRate.Should().Be(0);
    }

    [Fact]
    public void GetSuccessRate_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _metrics.GetSuccessRate(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAverageDurationMs_WithValidOperation_ReturnsCorrectAverage()
    {
        // Arrange
        const string operationName = "duration_test";
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: true);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(300), success: true);

        // Act
        var avgDuration = _metrics.GetAverageDurationMs(operationName);

        // Assert
        avgDuration.Should().Be(200);
    }

    [Fact]
    public void GetAverageDurationMs_WithNoOperations_Returns0()
    {
        // Arrange
        const string operationName = "no_duration_ops";

        // Act
        var avgDuration = _metrics.GetAverageDurationMs(operationName);

        // Assert
        avgDuration.Should().Be(0);
    }

    [Fact]
    public void GetAverageDurationMs_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _metrics.GetAverageDurationMs(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetFailureCount_WithValidOperation_ReturnsCorrectFailureCount()
    {
        // Arrange
        const string operationName = "failures";
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: true);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(200), success: false);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(150), success: false);

        // Act
        var failureCount = _metrics.GetFailureCount(operationName);

        // Assert
        failureCount.Should().Be(2);
    }

    [Fact]
    public void GetFailureCount_WithNoOperations_Returns0()
    {
        // Arrange
        const string operationName = "no_failures";

        // Act
        var failureCount = _metrics.GetFailureCount(operationName);

        // Assert
        failureCount.Should().Be(0);
    }

    [Fact]
    public void GetFailureCount_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _metrics.GetFailureCount(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetTotalCount_WithValidOperation_ReturnsCorrectTotalCount()
    {
        // Arrange
        const string operationName = "total_count";
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: true);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(200), success: true);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(150), success: false);

        // Act
        var totalCount = _metrics.GetTotalCount(operationName);

        // Assert
        totalCount.Should().Be(3);
    }

    [Fact]
    public void GetTotalCount_WithNoOperations_Returns0()
    {
        // Arrange
        const string operationName = "no_total_ops";

        // Act
        var totalCount = _metrics.GetTotalCount(operationName);

        // Assert
        totalCount.Should().Be(0);
    }

    [Fact]
    public void GetTotalCount_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _metrics.GetTotalCount(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetTotalDurationMs_WithValidOperation_ReturnsCorrectTotalDuration()
    {
        // Arrange
        const string operationName = "total_duration";
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: true);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(200), success: true);

        // Act
        var totalDuration = _metrics.GetTotalDurationMs(operationName);

        // Assert
        totalDuration.Should().Be(300);
    }

    [Fact]
    public void GetTotalDurationMs_WithNoOperations_Returns0()
    {
        // Arrange
        const string operationName = "no_total_duration_ops";

        // Act
        var totalDuration = _metrics.GetTotalDurationMs(operationName);

        // Assert
        totalDuration.Should().Be(0);
    }

    [Fact]
    public void GetTotalDurationMs_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _metrics.GetTotalDurationMs(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HasExecuted_WithExecutedOperation_ReturnsTrue()
    {
        // Arrange
        const string operationName = "executed_op";
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: true);

        // Act
        var hasExecuted = _metrics.HasExecuted(operationName);

        // Assert
        hasExecuted.Should().BeTrue();
    }

    [Fact]
    public void HasExecuted_WithNoOperations_ReturnsFalse()
    {
        // Arrange
        const string operationName = "not_executed";

        // Act
        var hasExecuted = _metrics.HasExecuted(operationName);

        // Assert
        hasExecuted.Should().BeFalse();
    }

    [Fact]
    public void HasExecuted_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _metrics.HasExecuted(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetLastExecutionTime_WithExecutedOperation_ReturnsCorrectTime()
    {
        // Arrange
        const string operationName = "last_time_op";
        var beforeTime = DateTime.UtcNow.AddSeconds(-1);
        _metrics.RecordOperation(operationName, TimeSpan.FromMilliseconds(100), success: true);
        var afterTime = DateTime.UtcNow.AddSeconds(1);

        // Act
        var lastExecutionTime = _metrics.GetLastExecutionTime(operationName);

        // Assert
        lastExecutionTime.Should().NotBeNull();
        lastExecutionTime.Should().BeAfter(beforeTime);
        lastExecutionTime.Should().BeBefore(afterTime);
    }

    [Fact]
    public void GetLastExecutionTime_WithNoOperations_ReturnsNull()
    {
        // Arrange
        const string operationName = "no_last_time";

        // Act
        var lastExecutionTime = _metrics.GetLastExecutionTime(operationName);

        // Assert
        lastExecutionTime.Should().BeNull();
    }

    [Fact]
    public void GetLastExecutionTime_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _metrics.GetLastExecutionTime(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetMostRecentOperation_WithMultipleOperations_ReturnsMostRecent()
    {
        // Arrange
        const string operation1 = "op1";
        const string operation2 = "op2";
        const string operation3 = "op3";

        _metrics.RecordOperation(operation1, TimeSpan.FromMilliseconds(100), success: true);
        _metrics.RecordOperation(operation2, TimeSpan.FromMilliseconds(200), success: true);
        _metrics.RecordOperation(operation3, TimeSpan.FromMilliseconds(150), success: true);

        // Act
        var mostRecent = _metrics.GetMostRecentOperation();

        // Assert
        mostRecent.Should().NotBeNull();
        mostRecent!.OperationName.Should().BeOneOf(operation1, operation2, operation3);
    }

    [Fact]
    public void GetMostRecentOperation_WithNoOperations_ReturnsNull()
    {
        // Act
        var mostRecent = _metrics.GetMostRecentOperation();

        // Assert
        mostRecent.Should().BeNull();
    }

    [Fact]
    public void GetOperationWithHighestFailureRate_WithMultipleOperations_ReturnsOperationWithMostFailures()
    {
        // Arrange
        const string highFailureOp = "high_failures";
        const string lowFailureOp = "low_failures";
        const string noFailureOp = "no_failures";

        _metrics.RecordOperation(highFailureOp, TimeSpan.FromMilliseconds(100), success: false);
        _metrics.RecordOperation(highFailureOp, TimeSpan.FromMilliseconds(200), success: false);
        _metrics.RecordOperation(highFailureOp, TimeSpan.FromMilliseconds(150), success: true);

        _metrics.RecordOperation(lowFailureOp, TimeSpan.FromMilliseconds(100), success: false);
        _metrics.RecordOperation(lowFailureOp, TimeSpan.FromMilliseconds(200), success: true);

        _metrics.RecordOperation(noFailureOp, TimeSpan.FromMilliseconds(100), success: true);

        // Act
        var operationWithHighestFailureRate = _metrics.GetOperationWithHighestFailureRate();

        // Assert
        operationWithHighestFailureRate.Should().Be(highFailureOp);
    }

    [Fact]
    public void GetOperationWithHighestFailureRate_WithNoOperations_ReturnsNull()
    {
        // Act
        var operationWithHighestFailureRate = _metrics.GetOperationWithHighestFailureRate();

        // Assert
        operationWithHighestFailureRate.Should().BeNull();
    }

    [Fact]
    public void GetAverageSuccessRate_WithMultipleOperations_ReturnsCorrectAverage()
    {
        // Arrange
        const string op1 = "op1";
        const string op2 = "op2";

        _metrics.RecordOperation(op1, TimeSpan.FromMilliseconds(100), success: true);
        _metrics.RecordOperation(op1, TimeSpan.FromMilliseconds(200), success: true);

        _metrics.RecordOperation(op2, TimeSpan.FromMilliseconds(150), success: false);
        _metrics.RecordOperation(op2, TimeSpan.FromMilliseconds(250), success: true);

        // Act
        var avgSuccessRate = _metrics.GetAverageSuccessRate();

        // Assert
        avgSuccessRate.Should().Be(75); // (100 + 50) / 2 = 75
    }

    [Fact]
    public void GetAverageSuccessRate_WithNoOperations_Returns0()
    {
        // Act
        var avgSuccessRate = _metrics.GetAverageSuccessRate();

        // Assert
        avgSuccessRate.Should().Be(0);
    }
}
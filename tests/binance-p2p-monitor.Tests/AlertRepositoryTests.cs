#nullable enable
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class AlertRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DatabaseContext _context;
    private readonly AlertRepository _alertRepository;

    public AlertRepositoryTests()
    {
        // Use an in-memory SQLite database for testing
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new DatabaseContext(_connection);
        _alertRepository = new AlertRepository(_context);

        // Initialize the database schema
        _context.ExecuteCommand(@"
            CREATE TABLE PriceAlerts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Asset TEXT NOT NULL,
                Fiat TEXT NOT NULL,
                AlertType INTEGER NOT NULL,
                Threshold REAL NOT NULL,
                Condition INTEGER NOT NULL,
                IsEnabled INTEGER NOT NULL,
                UserId INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                LastTriggeredAt INTEGER,
                TriggerCount INTEGER NOT NULL,
                Notes TEXT
            );");
        _context.ExecuteCommand(@"
            CREATE TABLE PriceHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Asset TEXT NOT NULL,
                Fiat TEXT NOT NULL,
                BuyPrice REAL NOT NULL,
                SellPrice REAL NOT NULL,
                SpreadPercentage REAL NOT NULL,
                RecordedAt TEXT NOT NULL
            );");
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    private PriceAlert CreateTestAlert(int userId = 1, string asset = "USDT", string fiat = "UAH")
    {
        return new PriceAlert
        {
            UserId = userId,
            Asset = asset,
            Fiat = fiat,
            AlertType = AlertType.PriceChange,
            Threshold = 1.0m,
            Condition = AlertCondition.GreaterThan,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TriggerCount = 0,
            Notes = "Test Alert"
        };
    }

    [Fact]
    public async Task AddAsync_ShouldAddAlertAndReturnId()
    {
        // Arrange
        var alert = CreateTestAlert();

        // Act
        var id = await _alertRepository.AddAsync(alert).ConfigureAwait(false);

        // Assert
        id.Should().BeGreaterThan(0);
        var storedAlert = await _alertRepository.GetByIdAsync(id).ConfigureAwait(false);
        storedAlert.Should().NotBeNull();
        storedAlert!.Asset.Should().Be(alert.Asset);
        storedAlert.Threshold.Should().Be(alert.Threshold);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAlert_WhenAlertExists()
    {
        // Arrange
        var alert = CreateTestAlert();
        var id = await _alertRepository.AddAsync(alert).ConfigureAwait(false);

        // Act
        var storedAlert = await _alertRepository.GetByIdAsync(id).ConfigureAwait(false);

        // Assert
        storedAlert.Should().NotBeNull();
        storedAlert!.Id.Should().Be(id);
        storedAlert.Asset.Should().Be(alert.Asset);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAlertDoesNotExist()
    {
        // Act
        var storedAlert = await _alertRepository.GetByIdAsync(999).ConfigureAwait(false);

        // Assert
        storedAlert.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAlertAndReturnTrue()
    {
        // Arrange
        var alert = CreateTestAlert();
        var id = await _alertRepository.AddAsync(alert).ConfigureAwait(false);
        var storedAlert = await _alertRepository.GetByIdAsync(id).ConfigureAwait(false);
        storedAlert!.Threshold = 2.0m;
        storedAlert.Notes = "Updated Test Alert";

        // Act
        var result = await _alertRepository.UpdateAsync(storedAlert).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        var updatedAlert = await _alertRepository.GetByIdAsync(id).ConfigureAwait(false);
        updatedAlert!.Threshold.Should().Be(2.0m);
        updatedAlert.Notes.Should().Be("Updated Test Alert");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenAlertDoesNotExist()
    {
        // Arrange
        var alert = CreateTestAlert();
        alert.Id = 999; // Non-existent ID

        // Act
        var result = await _alertRepository.UpdateAsync(alert).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAlertAndReturnTrue()
    {
        // Arrange
        var alert = CreateTestAlert();
        var id = await _alertRepository.AddAsync(alert).ConfigureAwait(false);

        // Act
        var result = await _alertRepository.DeleteAsync(id).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        var deletedAlert = await _alertRepository.GetByIdAsync(id).ConfigureAwait(false);
        deletedAlert.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenAlertDoesNotExist()
    {
        // Act
        var result = await _alertRepository.DeleteAsync(999).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserAlertsAsync_ShouldReturnAlertsForUser()
    {
        // Arrange
        var userId = 1;
        await _alertRepository.AddAsync(CreateTestAlert(userId)).ConfigureAwait(false);
        await _alertRepository.AddAsync(CreateTestAlert(userId)).ConfigureAwait(false);
        await _alertRepository.AddAsync(CreateTestAlert(userId: 2)).ConfigureAwait(false); // Another user's alert

        // Act
        var userAlerts = await _alertRepository.GetUserAlertsAsync(userId).ConfigureAwait(false);

        // Assert
        userAlerts.Should().HaveCount(2);
        userAlerts.Should().AllSatisfy(a => a.UserId.Should().Be(userId));
    }
}

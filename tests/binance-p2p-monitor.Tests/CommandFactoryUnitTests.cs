using BinanceP2pMonitor.CLI;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace BinanceP2pMonitor.Tests
{
    public class CommandFactoryUnitTests
    {
        [Fact]
        public void RegisterCommand_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange
            var commandFactory = new CommandFactory(It.IsAny<IServiceProvider>(), It.IsAny<ILogger<CommandFactory>>());

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => commandFactory.RegisterCommand(null, typeof(ICommand)));
        }

        [Fact]
        public void RegisterCommand_WithNullCommandType_ThrowsArgumentNullException()
        {
            // Arrange
            var commandFactory = new CommandFactory(It.IsAny<IServiceProvider>(), It.IsAny<ILogger<CommandFactory>>());

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => commandFactory.RegisterCommand("test", null));
        }

        [Fact]
        public void RegisterCommand_WithInvalidCommandType_ThrowsArgumentException()
        {
            // Arrange
            var commandFactory = new CommandFactory(It.IsAny<IServiceProvider>(), It.IsAny<ILogger<CommandFactory>>());

            // Act and Assert
            Assert.Throws<ArgumentException>(() => commandFactory.RegisterCommand("test", typeof(string)));
        }

        [Fact]
        public void CreateCommand_WithNullName_ReturnsNull()
        {
            // Arrange
            var commandFactory = new CommandFactory(It.IsAny<IServiceProvider>(), It.IsAny<ILogger<CommandFactory>>());

            // Act
            var command = commandFactory.CreateCommand(null);

            // Assert
            Assert.Null(command);
        }

        [Fact]
        public void CreateCommand_WithUnregisteredName_ReturnsNull()
        {
            // Arrange
            var commandFactory = new CommandFactory(It.IsAny<IServiceProvider>(), It.IsAny<ILogger<CommandFactory>>());

            // Act
            var command = commandFactory.CreateCommand("unregistered");

            // Assert
            Assert.Null(command);
        }

        [Fact]
        public void GetAvailableCommands_WithNoRegisteredCommands_ReturnsEmptyList()
        {
            // Arrange
            var commandFactory = new CommandFactory(It.IsAny<IServiceProvider>(), It.IsAny<ILogger<CommandFactory>>());

            // Act
            var availableCommands = commandFactory.GetAvailableCommands();

            // Assert
            Assert.Empty(availableCommands);
        }

        [Fact]
        public void IsCommandRegistered_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange
            var commandFactory = new CommandFactory(It.IsAny<IServiceProvider>(), It.IsAny<ILogger<CommandFactory>>());

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => commandFactory.IsCommandRegistered(null));
        }

        [Fact]
        public void IsCommandRegistered_WithUnregisteredName_ReturnsFalse()
        {
            // Arrange
            var commandFactory = new CommandFactory(It.IsAny<IServiceProvider>(), It.IsAny<ILogger<CommandFactory>>());

            // Act
            var isRegistered = commandFactory.IsCommandRegistered("unregistered");

            // Assert
            Assert.False(isRegistered);
        }
    }
}
#nullable enable

using BinanceP2pMonitor.CLI;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for the <see cref="CommandFactory"/> class.
/// </summary>
public class CommandFactoryUnitTests
{
    private readonly Mock<ILogger<CommandFactory>> _loggerMock = new();

    private CommandFactory CreateFactory()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        return new CommandFactory(serviceProvider, _loggerMock.Object);
    }

    [Fact]
    public void RegisterCommand_ValidCommand_CreatesCommandCaseInsensitively()
    {
        // Arrange
        var factory = CreateFactory();
        factory.RegisterCommand("SaMpLe", typeof(SampleCommand));

        // Act
        var command = factory.CreateCommand("SAMPLE");

        // Assert
        command.Should().BeOfType<SampleCommand>();
        factory.IsCommandRegistered("sample").Should().BeTrue();
    }

    [Fact]
    public void RegisterCommand_DuplicateName_ReplacesPreviousRegistration()
    {
        // Arrange
        var factory = CreateFactory();
        factory.RegisterCommand("sample", typeof(SampleCommand));

        // Act
        factory.RegisterCommand("SAMPLE", typeof(ReplacementCommand));

        // Assert
        factory.CreateCommand("sample").Should().BeOfType<ReplacementCommand>();
        factory.GetAvailableCommands().Should().ContainSingle().Which.Should().Be("sample");
    }

    [Fact]
    public void RegisterCommand_TypeThatDoesNotImplementICommand_ThrowsArgumentException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        Action act = () => factory.RegisterCommand("invalid", typeof(string));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*does not implement ICommand*");
    }

    [Fact]
    public void NameBasedMethods_NullName_ThrowNullReferenceException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        Action register = () => factory.RegisterCommand(null!, typeof(SampleCommand));
        Action create = () => factory.CreateCommand(null!);
        Action checkRegistration = () => factory.IsCommandRegistered(null!);

        // Assert
        register.Should().Throw<NullReferenceException>();
        create.Should().Throw<NullReferenceException>();
        checkRegistration.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void RegisterCommand_EmptyName_RegistersAndCreatesCommand()
    {
        // Arrange
        var factory = CreateFactory();
        factory.RegisterCommand(string.Empty, typeof(SampleCommand));

        // Act
        var command = factory.CreateCommand(string.Empty);

        // Assert
        command.Should().BeOfType<SampleCommand>();
        factory.IsCommandRegistered(string.Empty).Should().BeTrue();
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void CreateCommand_UnregisteredName_ReturnsNull(string commandName)
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var command = factory.CreateCommand(commandName);

        // Assert
        command.Should().BeNull();
    }

    [Fact]
    public void CreateCommand_UnresolvableConstructorDependency_ReturnsNull()
    {
        // Arrange
        var factory = CreateFactory();
        factory.RegisterCommand("broken", typeof(CommandWithDependency));

        // Act
        var command = factory.CreateCommand("broken");

        // Assert
        command.Should().BeNull();
    }

    [Fact]
    public void GetAvailableCommands_MultipleRegistrations_ReturnsNormalizedNames()
    {
        // Arrange
        var factory = CreateFactory();
        factory.RegisterCommand("FIRST", typeof(SampleCommand));
        factory.RegisterCommand("second", typeof(ReplacementCommand));

        // Act
        var commands = factory.GetAvailableCommands();

        // Assert
        commands.Should().BeEquivalentTo("first", "second");
        factory.IsCommandRegistered("unknown").Should().BeFalse();
    }

    public sealed class SampleCommand : ICommand
    {
        public string Name => "sample";
        public string Description => "A test command";
        public string GetHelp() => string.Empty;
        public List<string> ValidateArguments(CommandContext context) => [];
        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }

    public sealed class ReplacementCommand : ICommand
    {
        public string Name => "replacement";
        public string Description => "A replacement test command";
        public string GetHelp() => string.Empty;
        public List<string> ValidateArguments(CommandContext context) => [];
        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }

    public interface IMissingDependency;

    public sealed class CommandWithDependency(IMissingDependency dependency) : ICommand
    {
        public string Name => "broken";
        public string Description => dependency.ToString() ?? string.Empty;
        public string GetHelp() => string.Empty;
        public List<string> ValidateArguments(CommandContext context) => [];
        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }
}

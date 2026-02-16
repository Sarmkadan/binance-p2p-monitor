# Contributing to binance-p2p-monitor

First off, thank you for considering contributing to binance-p2p-monitor! It's people like you that make binance-p2p-monitor such a great tool.

## How to Contribute

### Reporting Bugs

If you have found a bug, please open a GitHub Issue. When filing an issue, make sure to answer these five questions:
1. What version of binance-p2p-monitor are you using?
2. What operating system are you using?
3. What did you do? (Please provide reproduction steps)
4. What did you expect to see?
5. What did you see instead?

### Submitting Pull Requests

1. **Fork** the repository on GitHub.
2. **Clone** your fork locally.
3. **Create a branch** for your feature or bug fix (`git checkout -b feature/your-feature-name` or `git checkout -b fix/your-bug-fix`).
4. **Make your changes** and ensure they follow the code style guidelines.
5. **Run the tests** to ensure everything is working correctly.
6. **Commit** your changes with clear and descriptive commit messages.
7. **Push** your branch to your fork on GitHub (`git push origin feature/your-feature-name`).
8. **Submit a Pull Request** to the main repository.

## Development Requirements

- **.NET 10.0 SDK** or later.

## Building Locally

```bash
# Clone the repository
git clone https://github.com/sarmkadan/binance-p2p-monitor.git
cd binance-p2p-monitor

# Restore dependencies
dotnet restore

# Build (Release configuration)
dotnet build --configuration Release

# Or use the provided build script
./build.sh          # Linux / macOS
build.bat           # Windows
```

## Running Tests

```bash
# Run all tests
dotnet test --configuration Release --verbosity normal

# Run tests with detailed output and results file
dotnet test --configuration Release --verbosity normal --logger "trx;LogFileName=test-results.trx"

# Run a specific test project
dotnet test tests/binance-p2p-monitor.Tests/ --configuration Release
```

## Running the Application

```bash
# Copy and edit configuration
cp appsettings.example.json appsettings.json

# Run monitor
dotnet run --project . -- monitor

# Run with Docker
docker-compose up --build
```

## Code Style

- Follow the existing conventions found in the codebase.
- Provide XML documentation comments for public APIs.
- **Important:** Keep all existing author headers intact. DO NOT remove them.

## License

By contributing to binance-p2p-monitor, you agree that your contributions will be licensed under its MIT License.

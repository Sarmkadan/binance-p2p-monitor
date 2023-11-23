#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
PROJECT_NAME="binance-p2p-monitor"
BUILD_CONFIG="${1:-Release}"
OUTPUT_DIR="./publish"
DOTNET_VERSION="10.0"

# Functions
print_header() {
    echo -e "${GREEN}===================================================${NC}"
    echo -e "${GREEN}$1${NC}"
    echo -e "${GREEN}===================================================${NC}"
}

print_info() {
    echo -e "${GREEN}[*]${NC} $1"
}

print_error() {
    echo -e "${RED}[!]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

check_dotnet() {
    print_info "Checking .NET SDK version..."
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK not found. Please install .NET $DOTNET_VERSION or later."
        exit 1
    fi

    INSTALLED_VERSION=$(dotnet --version)
    print_info ".NET version: $INSTALLED_VERSION"
}

restore_dependencies() {
    print_info "Restoring NuGet dependencies..."
    dotnet restore
    print_info "Dependencies restored."
}

build_debug() {
    print_info "Building Debug configuration..."
    dotnet build -c Debug --no-restore
    print_info "Debug build complete."
}

build_release() {
    print_info "Building Release configuration..."
    dotnet build -c Release --no-restore
    print_info "Release build complete."
}

run_tests() {
    print_info "Running unit tests..."
    if dotnet test -c Release --no-build --verbosity minimal; then
        print_info "All tests passed."
    else
        print_error "Tests failed!"
        exit 1
    fi
}

check_code_quality() {
    print_info "Checking code quality..."
    if dotnet format --verify-no-changes --verbosity quiet 2>/dev/null; then
        print_info "Code quality check passed."
    else
        print_warning "Code formatting issues found. Run 'dotnet format' to fix."
    fi
}

publish_self_contained() {
    print_info "Publishing self-contained binaries..."

    mkdir -p "$OUTPUT_DIR"

    # Linux
    print_info "Publishing for Linux (x64)..."
    dotnet publish -c Release -r linux-x64 --self-contained -o "$OUTPUT_DIR/linux-x64"

    # Windows
    print_info "Publishing for Windows (x64)..."
    dotnet publish -c Release -r win-x64 --self-contained -o "$OUTPUT_DIR/win-x64"

    # macOS
    print_info "Publishing for macOS (x64)..."
    dotnet publish -c Release -r osx-x64 --self-contained -o "$OUTPUT_DIR/osx-x64"

    print_info "Publishing complete. Binaries in: $OUTPUT_DIR/"
}

create_archives() {
    print_info "Creating distribution archives..."

    cd "$OUTPUT_DIR"

    # Linux archive
    if command -v tar &> /dev/null; then
        tar -czf "$PROJECT_NAME-linux-x64.tar.gz" linux-x64/
        print_info "Created: $PROJECT_NAME-linux-x64.tar.gz"
    fi

    # Windows archive
    if command -v zip &> /dev/null; then
        zip -r "$PROJECT_NAME-win-x64.zip" win-x64/
        print_info "Created: $PROJECT_NAME-win-x64.zip"
    elif command -v 7z &> /dev/null; then
        7z a "$PROJECT_NAME-win-x64.7z" win-x64/
        print_info "Created: $PROJECT_NAME-win-x64.7z"
    fi

    # macOS archive
    if command -v tar &> /dev/null; then
        tar -czf "$PROJECT_NAME-osx-x64.tar.gz" osx-x64/
        print_info "Created: $PROJECT_NAME-osx-x64.tar.gz"
    fi

    cd ..
}

build_docker_image() {
    print_info "Building Docker image..."
    if docker build -t "$PROJECT_NAME:latest" .; then
        print_info "Docker image built: $PROJECT_NAME:latest"
    else
        print_error "Docker build failed!"
        exit 1
    fi
}

main() {
    print_header "Binance P2P Monitor Build System"

    print_info "Build Configuration: $BUILD_CONFIG"

    # Check prerequisites
    check_dotnet

    # Restore
    restore_dependencies

    # Build
    if [ "$BUILD_CONFIG" = "Debug" ]; then
        build_debug
    else
        build_release
    fi

    # Run tests
    read -p "Run tests? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        run_tests
    fi

    # Code quality check
    check_code_quality

    # Publish self-contained
    read -p "Publish self-contained binaries? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        publish_self_contained

        read -p "Create distribution archives? (y/n) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            create_archives
        fi
    fi

    # Docker build
    read -p "Build Docker image? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        build_docker_image
    fi

    print_header "Build Complete!"
    print_info "Run: dotnet run -- --help"
}

# Parse arguments
case "${1:-}" in
    debug)
        BUILD_CONFIG="Debug"
        main
        ;;
    release)
        BUILD_CONFIG="Release"
        main
        ;;
    test)
        check_dotnet
        restore_dependencies
        build_release
        run_tests
        ;;
    clean)
        print_info "Cleaning build artifacts..."
        dotnet clean
        rm -rf "$OUTPUT_DIR"
        print_info "Clean complete."
        ;;
    docker)
        check_dotnet
        restore_dependencies
        build_release
        build_docker_image
        ;;
    publish)
        check_dotnet
        restore_dependencies
        build_release
        publish_self_contained
        create_archives
        ;;
    *)
        echo "Usage: $0 [command]"
        echo ""
        echo "Commands:"
        echo "  debug       Build in Debug mode (default)"
        echo "  release     Build in Release mode"
        echo "  test        Build and run tests"
        echo "  clean       Clean build artifacts"
        echo "  docker      Build Docker image"
        echo "  publish     Publish self-contained binaries"
        echo ""
        echo "Examples:"
        echo "  $0 release"
        echo "  $0 test"
        echo "  $0 publish"
        exit 1
        ;;
esac

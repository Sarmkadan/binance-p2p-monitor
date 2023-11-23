@echo off
REM =============================================================================
REM Author: Vladyslav Zaiets | https://sarmkadan.com
REM CTO & Software Architect
REM =============================================================================

setlocal enabledelayedexpansion

set PROJECT_NAME=binance-p2p-monitor
set BUILD_CONFIG=%1
if "%BUILD_CONFIG%"=="" set BUILD_CONFIG=Release
set OUTPUT_DIR=.\publish
set DOTNET_VERSION=10.0

echo.
echo ==================================================
echo Binance P2P Monitor Build System
echo ==================================================
echo.
echo Build Configuration: %BUILD_CONFIG%

REM Check .NET SDK
echo [*] Checking .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [!] .NET SDK not found. Please install .NET %DOTNET_VERSION% or later.
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VER=%%i
echo [*] .NET version: %DOTNET_VER%

REM Restore dependencies
echo [*] Restoring dependencies...
dotnet restore
if errorlevel 1 (
    echo [!] Restore failed!
    exit /b 1
)

REM Build
echo [*] Building %BUILD_CONFIG% configuration...
dotnet build -c %BUILD_CONFIG% --no-restore
if errorlevel 1 (
    echo [!] Build failed!
    exit /b 1
)

REM Run tests
choice /C YN /M "[?] Run unit tests"
if errorlevel 2 goto :skip_tests
if errorlevel 1 (
    echo [*] Running tests...
    dotnet test -c Release --no-build --verbosity minimal
    if errorlevel 1 (
        echo [!] Tests failed!
        exit /b 1
    )
)

:skip_tests
REM Publish
choice /C YN /M "[?] Publish self-contained binaries"
if errorlevel 2 goto :skip_publish
if errorlevel 1 (
    echo [*] Publishing...
    if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

    echo [*] Publishing for Windows (x64)...
    dotnet publish -c Release -r win-x64 --self-contained -o "%OUTPUT_DIR%\win-x64"
    if errorlevel 1 goto :publish_failed

    echo [*] Publishing for Linux (x64)...
    dotnet publish -c Release -r linux-x64 --self-contained -o "%OUTPUT_DIR%\linux-x64"
    if errorlevel 1 goto :publish_failed

    echo [*] Publishing for macOS (x64)...
    dotnet publish -c Release -r osx-x64 --self-contained -o "%OUTPUT_DIR%\osx-x64"
    if errorlevel 1 goto :publish_failed

    echo [*] Binaries published to: %OUTPUT_DIR%
)

:skip_publish
REM Docker
choice /C YN /M "[?] Build Docker image"
if errorlevel 2 goto :skip_docker
if errorlevel 1 (
    echo [*] Building Docker image...
    docker build -t %PROJECT_NAME%:latest .
    if errorlevel 1 (
        echo [!] Docker build failed!
        goto :end
    )
    echo [*] Docker image built: %PROJECT_NAME%:latest
)

:skip_docker
echo.
echo ==================================================
echo Build Complete!
echo ==================================================
echo.
echo Run: dotnet run -- --help
echo.
exit /b 0

:publish_failed
echo [!] Publishing failed!
exit /b 1

:end
endlocal

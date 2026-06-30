# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

# Copy project files
COPY binance-p2p-monitor.csproj .
COPY src/ src/
COPY appsettings.json .
COPY appsettings.development.json .

# Build application
RUN dotnet build -c Release --no-restore && \
    dotnet publish -c Release -o /app/publish --self-contained=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

# Copy published application
COPY --from=builder /app/publish .
COPY --from=builder /src/appsettings.json .
COPY --from=builder /src/appsettings.development.json .

# Create data and logs directories
RUN mkdir -p /app/data /app/logs

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV AppSettings__LogPath=/app/logs

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD ["dotnet", "/app/binance-p2p-monitor.dll", "status", "||", "exit", "1"]

EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "binance-p2p-monitor.dll"]
CMD ["monitor"]

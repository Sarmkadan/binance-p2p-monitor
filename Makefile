# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build release test clean install run docker-build docker-run fmt lint

PROJECT_NAME := binance-p2p-monitor
VERSION := $(shell grep 'Version' binance-p2p-monitor.csproj | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/')
DOTNET := dotnet
SHELL := /bin/bash

help:
	@echo "Binance P2P Monitor - Build & Development Commands"
	@echo ""
	@echo "Build targets:"
	@echo "  build          Build project in Debug mode"
	@echo "  release        Build project in Release mode"
	@echo "  clean          Clean build artifacts"
	@echo ""
	@echo "Testing & Quality:"
	@echo "  test           Run unit tests"
	@echo "  lint           Run code quality checks"
	@echo "  fmt            Format code"
	@echo ""
	@echo "Docker:"
	@echo "  docker-build   Build Docker image"
	@echo "  docker-run     Run in Docker container"
	@echo "  docker-stop    Stop Docker container"
	@echo ""
	@echo "Development:"
	@echo "  run            Run application"
	@echo "  watch          Run with file watching"
	@echo "  install        Install dependencies"
	@echo ""
	@echo "Deployment:"
	@echo "  publish        Publish self-contained binaries"
	@echo "  release        Create GitHub release (requires TAG=vX.X.X)"
	@echo ""

build:
	@echo "[*] Building Debug configuration..."
	$(DOTNET) build -c Debug

release:
	@echo "[*] Building Release configuration..."
	$(DOTNET) build -c Release

clean:
	@echo "[*] Cleaning build artifacts..."
	$(DOTNET) clean
	rm -rf bin obj *.nupkg

install:
	@echo "[*] Restoring dependencies..."
	$(DOTNET) restore

test:
	@echo "[*] Running tests..."
	$(DOTNET) test -c Release --no-build

lint:
	@echo "[*] Running code quality checks..."
	$(DOTNET) format --verify-no-changes

fmt:
	@echo "[*] Formatting code..."
	$(DOTNET) format

watch:
	@echo "[*] Running with file watcher..."
	$(DOTNET) watch run

run: build
	@echo "[*] Running application..."
	$(DOTNET) run

docker-build:
	@echo "[*] Building Docker image..."
	docker build -t $(PROJECT_NAME):latest \
		-t $(PROJECT_NAME):$(VERSION) \
		.

docker-run: docker-build
	@echo "[*] Running Docker container..."
	docker run -it --rm \
		-e AppSettings__TelegramBotToken="${TELEGRAM_BOT_TOKEN}" \
		-e AppSettings__TelegramAdminChatId="${TELEGRAM_ADMIN_CHAT_ID}" \
		-v $$(pwd)/data:/data \
		-v $$(pwd)/logs:/app/logs \
		$(PROJECT_NAME):latest monitor

docker-stop:
	@echo "[*] Stopping Docker container..."
	docker stop $$(docker ps -q -f ancestor=$(PROJECT_NAME):latest)

publish:
	@echo "[*] Publishing self-contained binaries..."
	@mkdir -p ./publish
	$(DOTNET) publish -c Release -r linux-x64 --self-contained -o ./publish/linux-x64
	$(DOTNET) publish -c Release -r win-x64 --self-contained -o ./publish/win-x64
	$(DOTNET) publish -c Release -r osx-x64 --self-contained -o ./publish/osx-x64
	@echo "[✓] Binaries available in ./publish/"

release-create:
	@if [ -z "$(TAG)" ]; then \
		echo "Usage: make release-create TAG=vX.X.X"; \
		exit 1; \
	fi
	@echo "[*] Creating release $(TAG)..."
	git tag -a $(TAG) -m "Release $(TAG)"
	git push origin $(TAG)
	@echo "[✓] Release $(TAG) created"

# Development targets
install-tools:
	@echo "[*] Installing development tools..."
	$(DOTNET) tool install -g dotnet-format --force
	$(DOTNET) tool install -g dotnet-audit --force

# Database maintenance
db-backup:
	@echo "[*] Backing up database..."
	@mkdir -p backups
	cp binance_p2p.db backups/binance_p2p_$$(date +%Y%m%d_%H%M%S).db
	@echo "[✓] Database backed up"

db-vacuum:
	@echo "[*] Vacuuming database..."
	sqlite3 binance_p2p.db "VACUUM; ANALYZE;"
	@echo "[✓] Database optimized"

db-reset:
	@echo "[!] Resetting database (this cannot be undone)..."
	@read -p "Are you sure? (y/N) " confirm; \
	if [ "$$confirm" = "y" ]; then \
		rm -f binance_p2p.db*; \
		echo "[✓] Database reset"; \
	fi

# Log management
logs-view:
	@echo "[*] Viewing application logs..."
	tail -f logs/*.log

logs-clean:
	@echo "[*] Cleaning old logs..."
	find logs -name "*.log" -mtime +7 -delete
	@echo "[✓] Old logs removed"

# CI/CD
ci: install test lint
	@echo "[✓] CI pipeline passed"

# Docker Compose
up:
	@echo "[*] Starting Docker Compose stack..."
	docker-compose up -d
	@echo "[✓] Stack started"

down:
	@echo "[*] Stopping Docker Compose stack..."
	docker-compose down
	@echo "[✓] Stack stopped"

logs:
	@echo "[*] Viewing Docker logs..."
	docker-compose logs -f

ps:
	@echo "[*] Docker Compose status..."
	docker-compose ps

# Version management
version:
	@echo "Version: $(VERSION)"

version-update:
	@if [ -z "$(NEW_VERSION)" ]; then \
		echo "Usage: make version-update NEW_VERSION=X.X.X"; \
		exit 1; \
	fi
	@echo "[*] Updating version to $(NEW_VERSION)..."
	@sed -i "s/<Version>.*<\/Version>/<Version>$(NEW_VERSION)<\/Version>/" \
		binance-p2p-monitor.csproj
	@echo "[✓] Version updated to $(NEW_VERSION)"

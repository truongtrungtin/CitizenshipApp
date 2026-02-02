#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
PROJECT="$ROOT_DIR/Tests/UiE2E.Playwright/UiE2E.Playwright.csproj"

DOTNET_CONFIGURATION=${DOTNET_CONFIGURATION:-Debug}

# Build test project to generate Playwright install script

dotnet build "$PROJECT" -c "$DOTNET_CONFIGURATION"

PLAYWRIGHT_DIR="$ROOT_DIR/Tests/UiE2E.Playwright/bin/$DOTNET_CONFIGURATION/net9.0"

if [[ -f "$PLAYWRIGHT_DIR/playwright.sh" ]]; then
  bash "$PLAYWRIGHT_DIR/playwright.sh" install
elif command -v pwsh >/dev/null 2>&1 && [[ -f "$PLAYWRIGHT_DIR/playwright.ps1" ]]; then
  pwsh "$PLAYWRIGHT_DIR/playwright.ps1" install
elif [[ -f "$PLAYWRIGHT_DIR/Microsoft.Playwright.dll" && -f "$PLAYWRIGHT_DIR/UiE2E.Playwright.runtimeconfig.json" ]]; then
  dotnet exec --runtimeconfig "$PLAYWRIGHT_DIR/UiE2E.Playwright.runtimeconfig.json" \
    "$PLAYWRIGHT_DIR/Microsoft.Playwright.dll" install
else
  echo "Playwright install script not found. Build output: $PLAYWRIGHT_DIR" >&2
  exit 1
fi

dotnet test "$PROJECT" -c "$DOTNET_CONFIGURATION" --no-build

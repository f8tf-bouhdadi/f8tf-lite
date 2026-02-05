#!/usr/bin/env bash
set -euo pipefail

MOD="DATA0_RawStore"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

CSPROJ="adapters/dotnet/src/DATA0_RawStore/DATA0_Store.csproj"
if [[ ! -f "$CSPROJ" ]]; then
  echo "[$MOD] ERROR: missing csproj: $CSPROJ" >&2
  exit 2
fi

command -v dotnet >/dev/null 2>&1 || { echo "[$MOD] ERROR: dotnet not found" >&2; exit 3; }

echo "[$MOD] build: START"
dotnet build "$CSPROJ" -c Release
echo "[$MOD] build: OK"

echo "[$MOD] run: START"
echo "[$MOD] If it starts, open: http://localhost:5270 (see console)\n[$MOD] Stop with Ctrl+C"
dotnet run --project "$CSPROJ" -c Release
echo "[$MOD] run: END"

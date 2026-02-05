#!/usr/bin/env bash
set -euo pipefail

MOD="DATA0_RawStore"
HOST="127.0.0.1"
PORT="${DATA0_PORT:-5270}"
CSPROJ="adapters/dotnet/src/DATA0_RawStore/DATA0_Store.csproj"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

LOG_DIR=".f8tf/evidence"
mkdir -p "$LOG_DIR"
LOG_FILE="$LOG_DIR/${MOD}.smoke.log"
: > "$LOG_FILE"

echo "[$MOD] smoke_v0: START host=$HOST port=$PORT" | tee -a "$LOG_FILE"

command -v dotnet >/dev/null 2>&1 || { echo "[$MOD] ERROR: dotnet not found" | tee -a "$LOG_FILE" >&2; exit 3; }
[[ -f "$CSPROJ" ]] || { echo "[$MOD] ERROR: missing csproj: $CSPROJ" | tee -a "$LOG_FILE" >&2; exit 2; }

echo "[$MOD] build: START" | tee -a "$LOG_FILE"
dotnet build "$CSPROJ" -c Release >>"$LOG_FILE" 2>&1
echo "[$MOD] build: OK" | tee -a "$LOG_FILE"

echo "[$MOD] run: START (background)" | tee -a "$LOG_FILE"
dotnet run --project "$CSPROJ" -c Release >>"$LOG_FILE" 2>&1 &
PID=$!

cleanup() {
  echo "[$MOD] cleanup: stopping pid=$PID" | tee -a "$LOG_FILE"
  kill "$PID" >/dev/null 2>&1 || true
  wait "$PID" >/dev/null 2>&1 || true
  echo "[$MOD] cleanup: done" | tee -a "$LOG_FILE"
}
trap cleanup EXIT

# Criterion A: log contains "Now listening on"
LISTEN_OK=0
# Criterion B: port is open
PORT_OK=0

for i in {1..30}; do
  if grep -q "Now listening on:" "$LOG_FILE"; then
    LISTEN_OK=1
  fi
  if (echo >/dev/tcp/${HOST}/${PORT}) >/dev/null 2>&1; then
    PORT_OK=1
  fi
  if [[ "$LISTEN_OK" -eq 1 && "$PORT_OK" -eq 1 ]]; then
    echo "[$MOD] smoke_v0: OK (log + port)" | tee -a "$LOG_FILE"
    exit 0
  fi
  sleep 0.3
done

echo "[$MOD] smoke_v0: FAIL" | tee -a "$LOG_FILE"
echo "[$MOD] criteria: log_listening=$LISTEN_OK port_open=$PORT_OK" | tee -a "$LOG_FILE"
echo "[$MOD] last log lines:" | tee -a "$LOG_FILE"
tail -n 80 "$LOG_FILE" | sed 's/\r$//' >&2
exit 2

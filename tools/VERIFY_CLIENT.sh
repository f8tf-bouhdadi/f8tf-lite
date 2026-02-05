#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACK="${1:-$ROOT/docs/PROOF_PACK_EXAMPLE}"

echo "[verify] pack=$PACK"

need() {
  local f="$1"
  [[ -e "$PACK/$f" ]] || { echo "[verify] ERROR missing: $f" >&2; exit 2; }
}

need "README_CLIENT.md"
need "MANIFEST.json"
need "SHA256SUMS"
need "EVIDENCE/verify.log"
need "SAMPLES/sample.ok.txt"

echo "[verify] structure: OK"

# checksum verification
(
  cd "$PACK"
  sha256sum -c SHA256SUMS
)
echo "[verify] checksums: OK"

# minimal evidence sanity
if ! grep -q "END: SUCCESS" "$PACK/EVIDENCE/verify.log"; then
  echo "[verify] ERROR evidence log does not contain END: SUCCESS" >&2
  exit 3
fi
echo "[verify] evidence: OK"

echo "[verify] END: SUCCESS"

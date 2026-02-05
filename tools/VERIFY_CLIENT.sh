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
need "EVIDENCE/VERIFY.txt"
need "SAMPLES/sample.ok.txt"

echo "[verify] structure: OK"

# checksum verification
(
  cd "$PACK"
  sha256sum -c SHA256SUMS
)
echo "[verify] checksums: OK"

# minimal evidence sanity
if [[ ! -s "$PACK/EVIDENCE/VERIFY.txt" ]]; then
  echo "[verify] ERROR evidence file is empty: EVIDENCE/VERIFY.txt" >&2
  exit 3
fi
echo "[verify] evidence: OK"
echo "[verify] END: SUCCESS"

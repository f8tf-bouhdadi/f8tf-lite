param(
  [string]$PackPath = "$(Split-Path -Parent $PSScriptRoot)\docs\PROOF_PACK_EXAMPLE"
)

Write-Host "[verify] pack=$PackPath"

function Need([string]$rel) {
  $p = Join-Path $PackPath $rel
  if (!(Test-Path $p)) { Write-Error "[verify] missing: $rel"; exit 2 }
}

Need "README_CLIENT.md"
Need "MANIFEST.json"
Need "SHA256SUMS"
Need "EVIDENCE\verify.log"
Need "SAMPLES\sample.ok.txt"

Write-Host "[verify] structure: OK"

# Verify SHA256SUMS
$lines = Get-Content (Join-Path $PackPath "SHA256SUMS")
foreach ($line in $lines) {
  if ($line.Trim().Length -eq 0) { continue }
  $parts = $line -split "\s+"
  $hash = $parts[0]
  $file = $parts[-1]
  $full = Join-Path $PackPath $file
  $calc = (Get-FileHash -Algorithm SHA256 $full).Hash.ToLower()
  if ($calc -ne $hash.ToLower()) {
    Write-Error "[verify] checksum mismatch: $file"
    exit 3
  }
}
Write-Host "[verify] checksums: OK"

$log = Get-Content (Join-Path $PackPath "EVIDENCE\verify.log") -Raw
if ($log -notmatch "END: SUCCESS") {
  Write-Error "[verify] evidence log missing END: SUCCESS"
  exit 4
}
Write-Host "[verify] evidence: OK"

Write-Host "[verify] END: SUCCESS"

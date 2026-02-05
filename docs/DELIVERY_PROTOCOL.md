# Delivery Protocol (Client)

## Step 1 — Qualification (15 min)
- Sector + risk level
- Target pack: Core / Identity & Trust / Platform
- Deadline + constraints (audit/regulation)

## Step 2 — One-page Offer
- Scope (modules)
- Level (V0/V1/V2)
- Deliverables
- Acceptance criteria (what client can verify)

## Step 3 — Delivery Package
Client receives an archive containing:
- README_CLIENT.md
- MANIFEST.json
- SHA256SUMS
- EVIDENCE/ (logs, gate outputs)
- SAMPLES/ (happy-path scenarios)
- VERIFY_CLIENT.(sh|ps1)

## Step 4 — Acceptance
Client runs VERIFY_CLIENT and returns:
- checksum OK
- verify output OK
- acceptance note / ticket reference

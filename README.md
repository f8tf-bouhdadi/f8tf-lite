F8TF Lite

F8TF = Formal Eight Transparencies & Functions (inspired by ISO RM-ODP).

This repository is the public Lite edition of the F8TF framework.
It provides verifiable delivery artefacts and minimal runnable demonstrations intended for technical evaluation and reproducibility.

No proprietary material, no confidential specifications, no commercial commitments.

Purpose

The goal of F8TF Lite is to demonstrate that modern application stacks
(.NET, Java/Spring, Python/Django, etc.) implement the same platform functions, such as:

data persistence

routing and processing

lifecycle management

basic reliability and observability

verifiable execution gates

This repository focuses on evidence, not features.

What this repository contains
1. Delivery & verification artefacts

Located in docs/ and tools/:

Offers overview (docs/OFFERS.md)

Delivery protocol (docs/DELIVERY_PROTOCOL.md)

Contact information (docs/CONTACT.md)

Public roadmap (docs/ROADMAP_PUBLIC.md)

Example proof pack (docs/PROOF_PACK_EXAMPLE/)

Client-side verification tools

tools/VERIFY_CLIENT.sh (Git Bash / Linux / macOS)

tools/VERIFY_CLIENT.ps1 (PowerShell / Windows)

These artefacts allow any reviewer to independently verify what is delivered.

2. Minimal runnable module (DATA0)

A single Lite demo module is included:

DATA0_RawStore

Purpose: minimal append-only data store

Scope: build + start + listen

No business logic

No credentials

No external dependencies

Location:

modules/DATA/DATA0_RawStore/


This module exists only to demonstrate:

reproducible build

controlled execution

smoke-level validation

Run a demo (DATA0_RawStore)

From the repository root:

bash modules/DATA/DATA0_RawStore/scripts/smoke_v0.sh


What this does:

builds the module

starts it in the background

verifies that the service listens on its TCP port

captures execution evidence

shuts it down cleanly

This is a V0 smoke test, not a functional benchmark.

Verification (delivery proof)

To verify a delivered proof pack example:

bash tools/VERIFY_CLIENT.sh


or on Windows (PowerShell):

tools\VERIFY_CLIENT.ps1


These scripts validate:

structure

checksums

manifest consistency

evidence presence

No network access is required.

What this repository is NOT

❌ Not a production-ready framework

❌ Not a SaaS offering

❌ Not a full specification

❌ Not a commercial contract

It is a public technical artefact for evaluation and discussion.

Pro edition (not included here)

The Pro edition includes:

complete specifications

formal models

extended modules

audit-grade documentation

structured delivery packs

Details are provided only on request.

Contact

Mohamed Bouhdadi
Professor & Researcher – Distributed Systems & Formal Methods

📧 Email: m.bouhdadi@um5r.ac.ma

🔗 LinkedIn: https://www.linkedin.com/in/mohamed-bouhdadi-4a784125b/

License

This repository is released under the terms specified in LICENSE.

Final note

This repository is intentionally minimal, explicit, and reproducible.
Everything present can be verified independently.
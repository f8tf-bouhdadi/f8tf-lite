[![DATA0 Smoke Test](https://github.com/f8tf-bouhdadi/f8tf-lite/actions/workflows/data0-smoke.yml/badge.svg?branch=main)](https://github.com/f8tf-bouhdadi/f8tf-lite/actions/workflows/data0-smoke.yml)

# data/ — Data pipeline (Lite)

Objectif : montrer que l’accès BD automatise un pipeline similaire au Web :
connexion → commande → mapping → résultat → erreurs.

## À venir
- D0 : ADO minimal (connection, command, reader)
- D1 : transactions/unité de travail (lite)
- D2 : mapping (DTO) + validations
- D3 : guard → gate (contraintes d’intégrité)

## Règle (Lite)
Aucune donnée réelle, aucun secret, pas de chaînes de connexion sensibles.

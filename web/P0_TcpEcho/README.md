# P0 — TCP Echo (bytes → message)

Objectif : exécuter un client + serveur TCP et voir :
- les octets envoyés/reçus (hex)
- le message décodé (UTF-8)
- un framing simple : [4 bytes longueur big-endian] + [payload UTF-8]

## Prérequis
- .NET SDK 8+

## Lancer le serveur
```bash
cd web/P0_TcpEcho
dotnet run -- server

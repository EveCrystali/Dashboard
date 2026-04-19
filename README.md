# Dashboard

Application mobile Android unifi\u00e9e agr\u00e9geant mes donn\u00e9es personnelles issues de **Notion** et de **Google Calendar** (via `CalendarContract` natif, sans API cloud).

## Vue d'ensemble

Un seul \u00e9cran, quatre widgets : **Aujourd'hui**, **Sant\u00e9**, **Candidatures actives**, **Journal r\u00e9cent**. Lecture seule. Sync incr\u00e9mental Notion. Lecture directe du calendrier Android local.

## Stack

| Couche | Choix |
|---|---|
| Runtime | .NET 10 LTS + C# 14 |
| UI | .NET MAUI 10 (Android, Windows en phase 2) |
| MVVM | CommunityToolkit.Mvvm |
| Persistance locale | EF Core 10 + Sqlite |
| HTTP | HttpClient typ\u00e9 + Polly |
| Logs | Serilog (sink fichier) |
| Tests | xUnit + Moq + FluentAssertions |
| CI | GitHub Actions |

## Structure de la solution

```
src/
  Dashboard.Core/    domaine, abstractions, insights d\u00e9terministes
  Dashboard.Data/    client Notion, EF Core, sync
  Dashboard.App/     MAUI Android (UI, plateforme, DI)
tests/
  Dashboard.Core.Tests/
  Dashboard.Data.Tests/
  Dashboard.App.Tests/
```

## Prise en main (d\u00e9veloppement)

### Pr\u00e9requis

- SDK .NET 10.0.1xx (voir `global.json`)
- Workloads : `maui-android`
- Android SDK avec API 35 install\u00e9e
- Un p\u00e9riph\u00e9rique ou \u00e9mulateur Android (API 26+)

### Configuration des secrets (dev)

```bash
cd src/Dashboard.App
dotnet user-secrets init
dotnet user-secrets set "Notion:IntegrationToken" "<ton_token_ici>"
```

**Jamais** de token en clair dans le d\u00e9p\u00f4t. En production (sur le device), le token est saisi via l'\u00e9cran de param\u00e9trage puis stock\u00e9 dans `SecureStorage`.

### Build

```bash
dotnet restore
dotnet build
dotnet test
```

### D\u00e9ploiement Android

```bash
dotnet build src/Dashboard.App -t:Run -f net10.0-android
```

## Accessibilit\u00e9

L'application est con\u00e7ue pour la **deut\u00e9ranopie** (daltonisme rouge/vert) :
- Aucune information cod\u00e9e uniquement par couleur rouge/vert.
- Palette signal\u00e9tique : rouge / orange / bleu + neutres.
- Chaque signal visuel est doubl\u00e9 d'une ic\u00f4ne ou d'un label texte.

## Documentation

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — vue d'ensemble
- [`docs/ADRs/`](docs/ADRs/) — d\u00e9cisions techniques (Architecture Decision Records)

## Licence

Projet personnel et portfolio. Non destin\u00e9 \u00e0 la distribution publique.

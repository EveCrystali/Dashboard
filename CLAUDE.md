# CLAUDE.md

Ce fichier fournit le contexte durable que **toute** session Claude Code (locale ou cloud) doit lire avant de contribuer au projet.

## Nature du projet

Application mobile Android **Dashboard**, d\u00e9velopp\u00e9e en .NET MAUI 10. Agr\u00e9ge les donn\u00e9es Notion (API REST) et Google Calendar (via `CalendarContract` Android natif, **pas d'API cloud**).

Double r\u00f4le :
1. Usage personnel quotidien d'Antoine.
2. Portfolio pour ses candidatures de d\u00e9veloppeur C# .NET \u2014 code propre, test\u00e9, document\u00e9.

## Stack impos\u00e9e

| Domaine | Choix | ADR |
|---|---|---|
| Runtime | .NET 10 LTS + C# 14 | \u2014 |
| UI | .NET MAUI 10 (`net10.0-android`, min API 26, target API 36.0) | \u2014 |
| MVVM | CommunityToolkit.Mvvm + DI MAUI natif | \u2014 |
| Persistance | EF Core 10 + Sqlite | [ADR-0001](docs/ADRs/0001-persistance-locale.md) |
| Mocking | Moq | [ADR-0002](docs/ADRs/0002-framework-mocking.md) |
| Navigation | Shell | [ADR-0003](docs/ADRs/0003-navigation-maui.md) |
| Calendrier testable | `ICalendarContentReader` POCO | [ADR-0004](docs/ADRs/0004-abstraction-contentresolver.md) |
| Multi-TFM | `.Core`/`.Data` en `net10.0`, `.App` en `net10.0-android` | [ADR-0005](docs/ADRs/0005-multi-tfm.md) |
| Logs | Serilog (sink fichier local) | \u2014 |
| Tests | xUnit + Moq + FluentAssertions | \u2014 |
| CI | GitHub Actions | \u2014 |
| Versions NuGet | Centralized Package Management (`Directory.Packages.props`) | \u2014 |

## R\u00e8gles absolues de s\u00e9curit\u00e9

**Ne JAMAIS demander la valeur r\u00e9elle d'un secret** (token Notion, cl\u00e9s API). Antoine ne les fournira sous aucun pr\u00e9texte.

- Interdit : cha\u00eenes litt\u00e9rales ressemblant \u00e0 un token (`"secret_xxxx"`, `"test_token_123"`). Utiliser `string.Empty` ou `null`.
- Interdit : un `appsettings.Development.json` contenant des secrets.
- Obligatoire : tout secret lu via `ITokenProvider` (SecureStorage en prod, user-secrets en Debug).
- Logs : jamais de token ni d'en-t\u00eate `Authorization` dans la sortie Serilog.

## Accessibilit\u00e9 non-n\u00e9gociable

Antoine est **daltonien (deut\u00e9ranopie)**. L'UI ne doit **jamais** coder une information uniquement par distinction rouge/vert. Palette signal\u00e9tique : rouge + orange + bleu + neutres. Chaque signal color\u00e9 est doubl\u00e9 d'une ic\u00f4ne ou d'un label texte.

## Structure de la solution

```
Dashboard.slnx              (.NET 10 solution au format XML)
\u251c\u2500\u2500 global.json              (SDK 10.0.100, rollForward latestFeature)
\u251c\u2500\u2500 Directory.Build.props   (LangVersion=14, Nullable=enable, TreatWarningsAsErrors=true)
\u251c\u2500\u2500 Directory.Packages.props (CPM centralis\u00e9)
\u251c\u2500\u2500 src/
\u2502   \u251c\u2500\u2500 Dashboard.Core/    (net10.0 \u2014 domaine, abstractions, insights)
\u2502   \u251c\u2500\u2500 Dashboard.Data/    (net10.0 \u2014 Notion client, EF Core, sync)
\u2502   \u2514\u2500\u2500 Dashboard.App/     (net10.0-android \u2014 MAUI UI, Platforms/Android)
\u2514\u2500\u2500 tests/
    \u251c\u2500\u2500 Dashboard.Core.Tests/ (net10.0)
    \u251c\u2500\u2500 Dashboard.Data.Tests/ (net10.0)
    \u2514\u2500\u2500 Dashboard.App.Tests/  (net10.0 \u2014 logique ViewModel framework-free)
```

Voir [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) pour la vue d\u00e9taill\u00e9e.

## Sources de donn\u00e9es

### Notion (`https://api.notion.com/v1/`, version `2022-06-28`+)

Auth : integration token interne personnel, lu via `ITokenProvider`.

Data sources d\u00e9j\u00e0 partag\u00e9es avec l'int\u00e9gration :
- T\u00e2ches : `f5e0c493-1e75-4d8b-bcdd-f16f7c4c0ee5`
- Candidatures : `ddd8f621-4493-465a-80c4-657d461c04b0`
- Journal Second Brain : `6a4776d3-f5f2-4dab-96de-98d66e5f3782`
- Health Monitor : `ff58ea49-daca-41da-9546-adbd93db18d6`
- Profil permanent (phase 2+) : `3303410f-2af0-813c-9792-ffc92683a130`

### Calendrier (Android `CalendarContract`)

Lecture 100 % locale via Content Provider. Aucun OAuth, aucun projet Google Cloud. Permission `READ_CALENDAR` d\u00e9clar\u00e9e dans `AndroidManifest.xml`, demand\u00e9e runtime via `Permissions.RequestAsync<Permissions.CalendarRead>()`.

## Avancement

Voir [`docs/PROGRESS.md`](docs/PROGRESS.md) pour l'\u00e9tat du projet lot par lot.

## Roadmap

Voir [`docs/ROADMAP.md`](docs/ROADMAP.md) pour la s\u00e9quence impos\u00e9e des 15 lots.

## Conventions

- **Commits** : Conventional Commits en fran\u00e7ais (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`).
- **Branches** : travail sur `main` (projet mono-d\u00e9veloppeur). Une PR par lot est optionnelle ; Antoine peut aussi commiter directement sur `main`.
- **Style C# 14** : file-scoped namespaces, records sealed, `var` quand le type est apparent, pattern matching moderne.
- **MVVM** : `[ObservableProperty]` et `[RelayCommand]` de `CommunityToolkit.Mvvm` ; aucun ViewModel ne r\u00e9f\u00e9rence `Microsoft.Maui.*` directement (testabilit\u00e9).

## Collaboration avec Claude

- **Style** : direct, sans pr\u00e9ambule ni flatterie.
- **Qualification \u00e9pist\u00e9mique** : Certain / Probable / Hypoth\u00e8se sur les affirmations non triviales.
- **Alternative + justification** sur tout choix technique discutable.
- **Questions explicites** au lieu de trancher silencieusement sur une d\u00e9cision qui rel\u00e8ve d'Antoine.
- **Ne jamais** demander la valeur d'un secret.

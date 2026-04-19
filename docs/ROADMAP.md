# Roadmap

S\u00e9quence impos\u00e9e des 15 lots. Chaque lot est une unit\u00e9 de travail de 0,5 \u00e0 1 jour de dev donnant lieu \u00e0 un commit (ou PR) avec un message Conventional Commits en fran\u00e7ais.

| # | Lot | Dur\u00e9e | Message de commit | \u00c9tat |
|---|-----|-------|-------------------|-------|
| 1 | Scaffolding solution + `global.json` + `Directory.Build.props` + CPM + `.gitignore` + CI GitHub Actions + ADRs | 0,5 j | `chore: scaffolding initial` | \u2705 |
| 2 | `ITokenProvider` + `SecureStorageTokenProvider` + user-secrets `#if DEBUG` + tests | 0,5 j | `feat: gestion des secrets` | \u23f3 |
| 3 | Mod\u00e8les de domaine (records sealed) + interfaces services + `IClock` | 0,5 j | `feat: domaine et abstractions` | |
| 4 | `NotionApiClient` (HttpClient typ\u00e9, `HttpMessageHandler` injectable, Polly retry, pagination) + tests | 1 j | `feat: client Notion bas niveau` | |
| 5 | `NotionService` : mapping des 4 data sources vers domaine + tests par fixture JSON | 1 j | `feat: mapping Notion` | |
| 6 | EF Core `AppDbContext` + migrations + repositories + tests SQLite in-memory | 1 j | `feat: persistance locale` | |
| 7 | `SyncOrchestrator` incr\u00e9mental + cursors + tests | 1 j | `feat: sync diff\u00e9rentielle Notion` | |
| 8 | `AndroidCalendarService` + `ICalendarContentReader` + permissions runtime + tests | 1 j | `feat: lecture CalendarContract` | |
| 9 | `CrossDomainInsightsService` (4 r\u00e8gles) + tests | 0,5 j | `feat: insights d\u00e9terministes` | |
| 10 | Shell + `DashboardPage` + 4 widgets vides + DI ViewModels | 1 j | `feat: squelette UI` | |
| 11 | Widgets Aujourd'hui + Sant\u00e9 (sparkline, palette daltonien-safe, ic\u00f4nes) | 1 j | `feat: widgets aujourd'hui et sant\u00e9` | |
| 12 | Widgets Candidatures + Journal | 0,5 j | `feat: widgets candidatures et journal` | |
| 13 | `SettingsPage` (token, permissions, toggles calendriers, seuils) | 1 j | `feat: \u00e9cran param\u00e8tres` | |
| 14 | Pull-to-refresh + Serilog fichier + gestion d'erreurs UI | 0,5 j | `feat: refresh et logs` | |
| 15 | Durcissement CI (couverture `coverlet` \u2265 70 % sur Core) + README portfolio + ADRs | 0,5 j | `docs: documentation portfolio` | |

**Total estim\u00e9 : ~11 jours de dev effectif.**

## R\u00e8gles d'ex\u00e9cution

1. **Ordre impos\u00e9** : ne pas sauter de lot sans validation explicite d'Antoine.
2. **Tests en parall\u00e8le du code** : chaque lot livre les tests unitaires correspondants (sauf Lot 1, scaffolding pur).
3. **ADR requis** pour tout nouveau choix technique non trivial rencontr\u00e9 en cours de route, ajout\u00e9 dans [`docs/ADRs/`](ADRs/).
4. **Mise \u00e0 jour** de [`PROGRESS.md`](PROGRESS.md) \u00e0 la fin de chaque lot.

## Hors scope phase 1 (rappel)

- Implementation Windows du calendrier (interface pr\u00e9vue, impl\u00e9mentation phase 2).
- Int\u00e9gration API Anthropic / analyse LLM (phase 3).
- \u00c9criture dans Notion ou le calendrier (lecture seule).
- Cible iOS.

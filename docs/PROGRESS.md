# Progress

\u00c9tat d'avancement du projet Dashboard. Mis \u00e0 jour \u00e0 la fin de chaque lot.

## Vue d'ensemble

| Lot | Livr\u00e9 le | Commit | \u00c9tat |
|---|---|---|---|
| 1. Scaffolding | 2026-04-19 | `chore: scaffolding initial` | \u2705 |
| 2. Gestion des secrets | | `feat: gestion des secrets` | \u23f3 prochain |
| 3\u201315 | | | |

## Lot 1 \u2014 Scaffolding (2026-04-19)

### Livrables

- Structure de solution `.slnx` (format XML .NET 10) avec 6 projets :
  - `src/Dashboard.Core` (net10.0)
  - `src/Dashboard.Data` (net10.0, r\u00e9f\u00e9rence \u2192 Core)
  - `src/Dashboard.App` (net10.0-android, r\u00e9f\u00e9rences \u2192 Core + Data)
  - `tests/Dashboard.Core.Tests` (net10.0)
  - `tests/Dashboard.Data.Tests` (net10.0)
  - `tests/Dashboard.App.Tests` (net10.0)
- `global.json` (SDK 10.0.100, rollForward latestFeature)
- `Directory.Build.props` (LangVersion=14, TreatWarningsAsErrors=true, Nullable=enable)
- `Directory.Packages.props` (CPM, versions centralis\u00e9es)
- `.gitignore` + `.editorconfig`
- `README.md`, `docs/ARCHITECTURE.md`, `docs/ROADMAP.md`, `docs/PROGRESS.md`, `CLAUDE.md`
- 5 ADRs dans `docs/ADRs/`
- `.github/workflows/ci.yml` \u2014 CI \u00e0 deux jobs (Ubuntu pour .Core/.Data/tests, Windows pour MAUI Android)
- Tests de fum\u00e9e `ScaffoldingTests.cs` dans chaque projet de tests

### V\u00e9rifications r\u00e9ussies

- `dotnet restore` + `dotnet build -c Release` sur `.Core`, `.Data` et les 3 tests : OK, 0 warning, 0 erreur.
- `dotnet test` sur les 3 tests : 3/3 passent.
- `dotnet build src/Dashboard.App -c Debug -f net10.0-android` : OK, 0 warning, 0 erreur.

### \u00c9carts assum\u00e9s par rapport au cahier des charges

1. **`TargetPlatformVersion` Android = 36.0** (cahier : 35). Le SDK install\u00e9 ne propose que 36.0 et 36.1. Respecte l'exigence "API 35 ou sup\u00e9rieur".
2. **`CommunityToolkit.Maui` retir\u00e9** du scaffold (version 11.0.0 incompatible MAUI 10 \u2014 conflit NU1107). \u00c0 r\u00e9introduire au Lot 10 avec la version compatible MAUI 10 (12.x en preview \u00e0 v\u00e9rifier).
3. **Fichiers iOS/MacCatalyst g\u00e9n\u00e9r\u00e9s** par `dotnet new maui` pr\u00e9sents mais non-compil\u00e9s (exclus via `TargetFrameworks=net10.0-android`). \u00c0 supprimer au besoin ; l'encombrement est n\u00e9gligeable.
4. **Correction `.gitignore`** : le pattern `*.app` matchait le dossier `Dashboard.App`. Remplac\u00e9 par `**/bin/**/*.app/` pour ne cibler que les bundles iOS dans les sorties de build.

### Ce qui reste \u00e0 faire au prochain lot (Lot 2)

- D\u00e9finir `ITokenProvider` dans `Dashboard.Core/Abstractions/`.
- Impl\u00e9mentation `SecureStorageTokenProvider` dans `Dashboard.App/Platforms/Android/Services/`.
- Initialiser `dotnet user-secrets` sur `Dashboard.App`.
- Brancher `builder.Configuration.AddUserSecrets<App>()` en `#if DEBUG` dans `MauiProgram.cs`.
- Tests unitaires sur le fallback Debug \u2194 SecureStorage.

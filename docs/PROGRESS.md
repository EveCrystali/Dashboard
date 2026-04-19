# Progress

\u00c9tat d'avancement du projet Dashboard. Mis \u00e0 jour \u00e0 la fin de chaque lot.

## Vue d'ensemble

| Lot | Livr\u00e9 le | Commit | \u00c9tat |
|---|---|---|---|
| 1. Scaffolding | 2026-04-19 | `chore: scaffolding initial` | \u2705 |
| 2. Gestion des secrets | 2026-04-19 | `feat: gestion des secrets` | \u2705 |
| 3. Domaine et abstractions | | `feat: domaine et abstractions` | \u23f3 prochain |
| 4\u201315 | | | |

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

Termin\u00e9, voir section ci-dessous.

## Lot 2 \u2014 Gestion des secrets (2026-04-19)

### Livrables

- `Dashboard.Core/Abstractions/ITokenProvider.cs` \u2014 interface publique (`GetNotionTokenAsync` / `SetNotionTokenAsync` / `ClearAsync`).
- `Dashboard.Core/Abstractions/ISecureStorageWrapper.cs` \u2014 abstraction testable au-dessus de `SecureStorage.Default`, unique point de couplage MAUI.
- `Dashboard.Core/Services/SecureStorageTokenProvider.cs` \u2014 impl\u00e9mentation `ITokenProvider` adoss\u00e9e au wrapper (logique pure, **aucune** d\u00e9pendance MAUI).
- `Dashboard.Core/Services/CompositeTokenProvider.cs` \u2014 d\u00e9corateur fallback `IConfiguration` activ\u00e9 uniquement `#if DEBUG`. \u00c9critures et purge d\u00e9l\u00e9gu\u00e9es au primaire.
- `Dashboard.App/Platforms/Android/Services/SecureStorageWrapper.cs` \u2014 impl\u00e9mentation concr\u00e8te plateforme (`SecureStorage.Default`), `internal sealed`.
- `Dashboard.App.csproj` : ajout d'un `UserSecretsId` pour activer `dotnet user-secrets`.
- `MauiProgram.cs` : `builder.Configuration.AddUserSecrets<App>(optional: true)` en `#if DEBUG`, enregistrements DI (primary concret + r\u00e9solveur `ITokenProvider` branch\u00e9 sur `CompositeTokenProvider` en Debug, directement sur `SecureStorageTokenProvider` en Release).
- `Directory.Packages.props` + `Dashboard.Core.csproj` : ajout de `Microsoft.Extensions.Configuration.Abstractions` (n\u00e9cessaire pour `CompositeTokenProvider`).
- Tests unitaires `Dashboard.Core.Tests/Services/` :
  - `SecureStorageTokenProviderTests` (4 tests) \u2014 lecture pr\u00e9sente, lecture vide (\u2261 null en Release), set persiste, clear supprime.
  - `CompositeTokenProviderTests` (6 tests) \u2014 primaire prioritaire, fallback config si primaire null, fallback si primaire vide, null si tout vide, set/clear d\u00e9l\u00e9gu\u00e9s.

### V\u00e9rifications

- Build et tests \u00e0 valider via la CI GitHub Actions (le sandbox de d\u00e9veloppement ne dispose pas du SDK `dotnet` pour une v\u00e9rification locale).
- Aucune cha\u00eene litt\u00e9rale ressemblant \u00e0 un token dans le code ni dans les tests (valeurs d'exemple : `"valeur-attendue"`, `"valeur-primaire"`, `"valeur-config"`, `"nouvelle-valeur"`).

### \u00c9carts assum\u00e9s par rapport au cahier du Lot 2

1. **`SecureStorageTokenProvider` plac\u00e9 dans `Dashboard.Core/Services/`** au lieu de `Dashboard.App/Platforms/Android/Services/`. Motif : `Dashboard.App.Tests` cible `net10.0` pur et ne peut pas r\u00e9f\u00e9rencer `Dashboard.App` (`net10.0-android`). Le provider ne d\u00e9pendant que de `ISecureStorageWrapper`, il reste de la logique pure \u2014 conforme \u00e0 l'esprit de l'ADR-0005 ("les abstractions vivent dans `.Core`, les impl\u00e9mentations plateforme dans `.App/Platforms/Android/`"). Seul `SecureStorageWrapper` (qui touche effectivement `SecureStorage.Default`) reste dans `Platforms/Android/Services/`.
2. **Tests \u00e9crits dans `Dashboard.Core.Tests`** au lieu de `Dashboard.App.Tests`. Corollaire direct du point 1 : le code test\u00e9 vit dans Core.
3. **`SetNotionTokenAsync` ne valide pas son argument**. Choix explicite \u2014 la validation UI sera ajout\u00e9e au Lot 13 (`SettingsPage`).
4. **Constantes de cl\u00e9s (`NotionTokenKey`, `NotionTokenConfigurationKey`) expos\u00e9es via `InternalsVisibleTo`** plut\u00f4t qu'\u00e0 pass\u00e9es `public`. Permet de garder l'API publique propre tout en autorisant les tests \u00e0 v\u00e9rifier les cl\u00e9s exactes.
5. **Job CI `Build MAUI Android` bascul\u00e9 de Release vers Debug** apr\u00e8s \u00e9chec `NETSDK1144` (ILLink / trimming) en Release. La cause pr\u00e9cise (warning IL2xxx promu en erreur par `TreatWarningsAsErrors=true`) n'a pas \u00e9t\u00e9 diagnostiqu\u00e9e. D\u00e9cision explicite d'Antoine : Debug suffit pour la phase 1 "\u00e7a compile" ; l'investigation et le durcissement Release sont report\u00e9s au **Lot 15** (durcissement CI + couverture). \u00c0 traiter avant toute publication Release.

### Ce qui reste \u00e0 faire au prochain lot (Lot 3)

- Mod\u00e8les de domaine (records `sealed`) pour les 4 data sources Notion.
- Abstractions `IClock`, interfaces de services m\u00e9tier.
- Tests correspondants.

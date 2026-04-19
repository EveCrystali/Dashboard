# Progress

\u00c9tat d'avancement du projet Dashboard. Mis \u00e0 jour \u00e0 la fin de chaque lot.

## Vue d'ensemble

| Lot | Livr\u00e9 le | Commit | \u00c9tat |
|---|---|---|---|
| 1. Scaffolding | 2026-04-19 | `chore: scaffolding initial` | \u2705 |
| 2. Gestion des secrets | 2026-04-19 | `feat: gestion des secrets` | \u2705 |
| 3. Domaine et abstractions | 2026-04-19 | `feat: domaine et abstractions` | \u2705 |
| 4. Client Notion bas niveau | 2026-04-19 | `feat: client Notion bas niveau` | \u2705 |
| 5. Mapping Notion | | `feat: mapping Notion` | \u23f3 prochain |
| 6\u201315 | | | |

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

Termin\u00e9, voir section ci-dessous.

## Lot 3 \u2014 Domaine et abstractions (2026-04-19)

### Livrables

- `Dashboard.Core/Domain/` \u2014 4 records `sealed` minimaux, un par data source Notion :
  - `TodoItem` (Id, Title, Due, IsCompleted).
  - `JobApplication` (Id, Company, Role, Status, AppliedAt).
  - `JournalEntry` (Id, Date, Title, Content).
  - `HealthReading` (Id, Date, HeartRateVariability, RestingHeartRate, SleepScore).
- `Dashboard.Core/Abstractions/IClock.cs` \u2014 interface minimale (`DateTimeOffset Now`).
- `Dashboard.Core/Services/SystemClock.cs` \u2014 impl\u00e9mentation `DateTimeOffset.Now`.
- `Dashboard.Core.Tests/Services/SystemClockTests.cs` \u2014 1 test (lecture proche de `DateTimeOffset.Now` \u00e0 5 s pr\u00e8s).

### Choix assum\u00e9s

1. **Pas d'interfaces de services m\u00e9tier \u00e0 ce lot**. Le cahier ROADMAP mentionne "interfaces services" au pluriel ; arbitrage explicite d'Antoine : "limite au n\u00e9cessaire, on fera \u00e9voluer plus tard". Les interfaces (`INotionService`, `ICalendarContentReader`, `ISyncOrchestrator`\u2026) seront ajout\u00e9es au lot o\u00f9 elles sont impl\u00e9ment\u00e9es (Lots 4\u20138).
2. **Records minimaux**. Champs choisis par \u00e9vidence fonctionnelle (ex. `HeartRateVariability` pour le seuil HRV <53 ms). Ajustements \u00e0 pr\u00e9voir aux Lots 4\u20135 lors du mapping r\u00e9el des data sources Notion.
3. **`IClock`** minimal : seulement `Now`. Pas de `Today` / `UtcNow` tant qu'aucun service n'en a besoin.
4. **Pas de tests sur les records de domaine**. Triviaux : \u00e9galit\u00e9 structurelle v\u00e9rifi\u00e9e par le compilateur, pas de logique m\u00e9tier. Seul `SystemClock` est test\u00e9.

### Ce qui reste \u00e0 faire au prochain lot (Lot 4)

Termin\u00e9, voir section ci-dessous.

## Lot 4 \u2014 Client Notion bas niveau (2026-04-19)

### Livrables

- `Dashboard.Data/Notion/NotionOptions.cs` \u2014 options li\u00e9es \u00e0 la section `Notion` d'`IConfiguration` (`BaseAddress`, `NotionVersion`).
- `Dashboard.Data/Notion/NotionPage.cs` \u2014 DTO typ\u00e9 au niveau enveloppe (`Id`, `CreatedTime`, `LastEditedTime`, `Archived`) + dictionnaire brut `Properties : IReadOnlyDictionary<string, JsonElement>` ; l'interpr\u00e9tation des types de propri\u00e9t\u00e9s est le travail du Lot 5.
- `Dashboard.Data/Notion/NotionQueryResponse.cs` \u2014 DTO de r\u00e9ponse (`Results`, `NextCursor`, `HasMore`).
- `Dashboard.Data/Notion/NotionAuthenticationHandler.cs` \u2014 `DelegatingHandler` qui injecte `Authorization: Bearer {token}` via `ITokenProvider` (relu \u00e0 chaque requ\u00eate, supporte la rotation) et l'en-t\u00eate `Notion-Version`.
- `Dashboard.Data/Notion/NotionApiClient.cs` \u2014 HttpClient typ\u00e9. M\u00e9thode bas niveau `QueryDataSourceOneBatchAsync(dataSourceId, startCursor)` + m\u00e9thode haut niveau `QueryDataSourceAsync(dataSourceId)` en `IAsyncEnumerable<NotionPage>` qui itère jusqu'\u00e0 `HasMore = false`.
- `Dashboard.Data/Notion/NotionServiceCollectionExtensions.cs` \u2014 extension `AddNotionClient(IServiceCollection, IConfiguration)` : `Configure<NotionOptions>`, enregistrement du handler, `AddHttpClient<NotionApiClient>` avec `AddStandardResilienceHandler` (timeout + retry avec backoff+jitter + circuit breaker, fourni par `Microsoft.Extensions.Http.Resilience`).
- `Directory.Packages.props` + `Dashboard.Data.csproj` : ajout de `Microsoft.Extensions.Http.Resilience` 10.0.0 et `Microsoft.Extensions.Options` 10.0.0.
- Tests unitaires `Dashboard.Data.Tests/Notion/` :
  - `FakeHttpMessageHandler` utilitaire.
  - `NotionAuthenticationHandlerTests` (3 tests) : ajout des headers quand token pr\u00e9sent ; pas d'`Authorization` si token vide ; relecture du token \u00e0 chaque requ\u00eate.
  - `NotionApiClientTests` (4 tests) : POST sans cursor + d\u00e9s\u00e9rialisation ; POST avec `start_cursor` ; it\u00e9ration multi-batches (3 pages), v\u00e9rification des cursors propag\u00e9s ; `HttpRequestException` si status non 2xx.

### Choix assum\u00e9s

1. **Endpoint `databases/{id}/query` + version `2022-06-28`**. Les UUIDs fournis dans `CLAUDE.md` sont nomm\u00e9s "data sources" mais leur origine exacte (database id vs. data source id API r\u00e9cente) n'est pas connue. Configuration centralis\u00e9e dans `NotionOptions` : bascule triviale au Lot 5 si un appel r\u00e9el \u00e9choue.
2. **Propri\u00e9t\u00e9s en `JsonElement`** dans `NotionPage` plut\u00f4t qu'en DTO exhaustifs par type. La surface API Notion pour les propri\u00e9t\u00e9s est tr\u00e8s large (title, rich_text, date, number, select, status, checkbox, people, files\u2026) et chaque data source n'utilise qu'un petit sous-ensemble. Le mapping typ\u00e9 se fera au Lot 5, cibl\u00e9 sur les 4 data sources r\u00e9elles.
3. **Retry Polly via `AddStandardResilienceHandler`** (d\u00e9fauts MS) plut\u00f4t qu'une politique custom. Gagne timeouts, retry, circuit breaker "gratuitement". Tests d'int\u00e9gration du retry non inclus : la politique est fournie et test\u00e9e par Microsoft.
4. **Aucune interface `INotionClient` ou `INotionService`** introduite \u00e0 ce lot (cf. d\u00e9cision du Lot 3). `NotionApiClient` est utilis\u00e9 directement ; une interface sera ajout\u00e9e au Lot 5 si le mapping service l'exige.

### Ce qui reste \u00e0 faire au prochain lot (Lot 5)

Termin\u00e9, voir section ci-dessous.

## Lot 5 \u2014 Mapping Notion (2026-04-19)

### Livrables

- **Domaine enrichi** (`Dashboard.Core/Domain/`) :
  - `DateRange` (record `Start`, `End`, `IsDateTime`) pour repr\u00e9senter les dates Notion (date simple, datetime, plage).
  - `TodoItem`, `JobApplication`, `JournalEntry`, `HealthReading` r\u00e9\u00e9crits avec les champs r\u00e9els des 4 bases Notion.
  - Enums : `TodoStatus`/`TodoPriority`/`TodoTag` ; `JobAppStatus`/`JobPosition`/`CompanyType`/`JobInterest`/`ContactMethod` ; `JournalType`/`JournalDomain`/`JournalSource` ; `HealthEntryType`/`HealthVerdict`/`HealthSource`/`ExerciseType`.
  - `HealthDailySnapshot` + `HealthAlert` : projection agr\u00e9g\u00e9e pour le widget Sant\u00e9 (production diff\u00e9r\u00e9e \u00e0 un lot ult\u00e9rieur).
- **Abstraction de lecture bas niveau** (`Dashboard.Core/Notion/`) :
  - `INotionPropertyReader` + impl `NotionPropertyReader` : `AsTitle`, `AsRichText`, `AsDate`, `AsSelect`, `AsStatus`, `AsMultiSelect`, `AsNumber`, `AsCheckbox`, `AsRelation`, `AsPeople`, `AsUrl`, `AsFiles`. Tol\u00e9rant aux formes absentes / null.
  - `NotionPropertyExtensions` : helpers d'extension sur `IReadOnlyDictionary<string, JsonElement>` (cl\u00e9 absente \u2192 valeur par d\u00e9faut, jamais d'exception).
  - `NotionPage` et `NotionQueryResponse` **d\u00e9plac\u00e9s** de `Dashboard.Data.Notion` vers `Dashboard.Core.Notion` pour que les mappers Core puissent les consommer sans d\u00e9pendance HTTP.
- **Mappers statiques** (`Dashboard.Core/Notion/Mappers/`) : `TodoMapper`, `JobApplicationMapper`, `JournalEntryMapper`, `HealthReadingMapper`. Noms de colonnes Notion expos\u00e9s en `const` publiques (`ColumnTitle`, etc.). Dictionnaires statiques pour la correspondance valeurs Notion (avec emojis et apostrophes typographiques exacts) \u2194 enums C#.
- **Orchestrateur haut niveau** (`Dashboard.Data/Notion/NotionService.cs`) : 4 m\u00e9thodes `IAsyncEnumerable<T> Get*Async()` qui enchaînent `NotionApiClient.QueryDataSourceAsync(...)` + le mapper ad\u00e9quat.
- **Options enrichies** : `NotionOptions.DataSources` (`NotionDataSources` avec les 4 UUIDs par d\u00e9faut, override possible via configuration).
- **DI** : `AddNotionClient` enregistre en plus `INotionPropertyReader` (singleton) et `NotionService` (singleton).
- **Tests** :
  - `Dashboard.Core.Tests/Notion/NotionPropertyReaderTests.cs` : 20 tests unitaires (JSON inline) couvrant tous les types Notion, y compris les cas limites (array vide, `null` explicite, date simple vs datetime, plage).
  - `Dashboard.Core.Tests/Notion/Mappers/` : un test par mapper utilisant des fixtures JSON r\u00e9alistes dans `Notion/Fixtures/` (`todo-page.json`, `job-application-page.json`, `journal-entry-page.json`, `health-reading-page.json`, copi\u00e9es en output via `CopyToOutputDirectory`).
  - `Dashboard.Core.Tests/Notion/Mappers/TodoMapperTests.cs` : tol\u00e9rance aux statuts et tags inconnus.
  - `Dashboard.Data.Tests/Notion/NotionServiceTests.cs` : v\u00e9rifie que chaque m\u00e9thode cible le bon data source ID via l'URL appel\u00e9e.

### Choix assum\u00e9s

1. **`TaskStatus` renomm\u00e9 `TodoStatus`** (et `TaskPriority` \u2192 `TodoPriority`, `TaskTag` \u2192 `TodoTag`). Collision avec `System.Threading.Tasks.TaskStatus` import\u00e9 via `ImplicitUsings`. Le pr\u00e9fixe `Todo*` est coh\u00e9rent avec `TodoItem` et \u00e9vite un `using` qualifiant dans tout le code.
2. **`NotionPage` / `NotionQueryResponse` d\u00e9plac\u00e9s de Data vers Core** pour \u00e9viter que `Dashboard.Core` d\u00e9pende de `Dashboard.Data` (cycle). Les DTOs sont des structures de donn\u00e9es neutres, sans d\u00e9pendance HTTP.
3. **Valeurs d'enums inconnues tol\u00e9r\u00e9es \u00e0 la lecture** : si une option select/status/multi_select n'est pas dans le dictionnaire, elle est ignor\u00e9e (multi) ou renvoy\u00e9e `null` (select). Le mapper ne jette pas : \u00e9vite les crashes quand une nouvelle option est ajout\u00e9e c\u00f4t\u00e9 Notion en cours de journ\u00e9e. Contrepartie : un log est \u00e0 ajouter au Lot 7 (sync orchestrator) pour alerter Antoine.
4. **Fallback `TodoStatus.Tache`** quand le statut lu est absent ou inconnu (tests couvrent ce cas). Pour les autres enums principaux (`JobAppStatus`), fallback `PretAPostuler`. D\u00e9cision arbitraire mais non bloquante ; ajustable si Antoine pr\u00e9f\u00e8re nullable partout.
5. **Helpers `MapEnum` / `MapEnumNullable` / `MapMulti` dupliqu\u00e9s dans 3 mappers**. Facturer une factorisation interne (par ex. dans `NotionPropertyExtensions`) coûterait plus en lisibilit\u00e9 qu'elle ne rapporte : 10 lignes \u00d7 3 fichiers, signature clairement locale.
6. **Apostrophes typographiques (U+2019) obligatoires** dans `TodoMapper` (« Date d\u2019\u00e9ch\u00e9ance », « R\u00e9sum\u00e9 g\u00e9n\u00e9r\u00e9 par l\u2019IA ») vs apostrophe droite (U+0027) dans `JobApplicationMapper` (« Date d'\u00e9ch\u00e9ance »). Caract\u00e8res pr\u00e9serv\u00e9s via \u00e9chappements `\u2019` pour \u00e9viter tout doute visuel.
7. **`ParentUrl` et `SubtaskUrls` stockent des IDs Notion bruts** (r\u00e9sultat de `relation.id`). Le nommage « Url » vient du sch\u00e9ma Notion d'Antoine ; la transformation ID \u2192 URL Notion (`https://www.notion.so/{id-sans-tiret}`) est pr\u00e9sum\u00e9e UI, pas domaine.
8. **`HealthDailySnapshot` cr\u00e9\u00e9 mais pas produit** \u00e0 ce lot. L'agr\u00e9gation (prendre la derni\u00e8re `Sommeil`, la derni\u00e8re `Poids`, calculer les alertes) sera faite au Lot 6 ou 11 selon le besoin UI.

### Ce qui reste \u00e0 faire au prochain lot (Lot 6)

- EF Core `AppDbContext` + migrations initiales + repositories pour les 4 records + tests SQLite in-memory.

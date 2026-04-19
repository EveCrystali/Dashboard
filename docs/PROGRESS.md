# Progress

\u00c9tat d'avancement du projet Dashboard. Mis \u00e0 jour \u00e0 la fin de chaque lot.

## Vue d'ensemble

| Lot | Livr\u00e9 le | Commit | \u00c9tat |
|---|---|---|---|
| 1. Scaffolding | 2026-04-19 | `chore: scaffolding initial` | \u2705 |
| 2. Gestion des secrets | 2026-04-19 | `feat: gestion des secrets` | \u2705 |
| 3. Domaine et abstractions | 2026-04-19 | `feat: domaine et abstractions` | \u2705 |
| 4. Client Notion bas niveau | 2026-04-19 | `feat: client Notion bas niveau` | \u2705 |
| 5. Mapping Notion | 2026-04-19 | `feat: mapping Notion` | \u2705 |
| 6. Persistance locale | 2026-04-19 | `feat: persistance locale` | \u2705 |
| 7. Sync diff\u00e9rentielle Notion | 2026-04-19 | `feat: sync diff\u00e9rentielle Notion` | \u2705 |
| 8. Lecture CalendarContract | 2026-04-19 | `feat: lecture CalendarContract` | \u23f3 en cours |
| 9\u201315 | | | |

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

Termin\u00e9, voir section ci-dessous.

## Lot 6 \u2014 Persistance locale (2026-04-19)

### Livrables

- **ADR-0006** \u2014 P\u00e9rim\u00e8tre de la persistance : cache de Notion (pas source de v\u00e9rit\u00e9) + entit\u00e9s EF s\u00e9par\u00e9es des records + collections en JSON dans des colonnes TEXT.
- **Interfaces repositories** (`Dashboard.Core/Abstractions/`) : `ITodoRepository`, `IJobApplicationRepository`, `IJournalEntryRepository`, `IHealthReadingRepository`, `ISyncCursorStore` (+ record `SyncCursor`). Contrat unifi\u00e9 : `GetAllAsync`, `UpsertAsync(item, lastEditedTime)`, `DeleteMissingAsync(presentIds)`.
- **Entit\u00e9s EF plates** (`Dashboard.Data/Persistence/Entities/`) : `TodoEntity`, `JobApplicationEntity`, `JournalEntryEntity`, `HealthReadingEntity`, `SyncCursorEntity`. Collections s\u00e9rialis\u00e9es en `*Json` (string), `DateRange` \u00e9clat\u00e9 en trois colonnes plates (`*Start`, `*End`, `*IsDateTime`).
- **Configurations Fluent API** (`Dashboard.Data/Persistence/Configurations/`) : une par entit\u00e9, cl\u00e9 primaire `Id`, conversions enums \u2192 `int`, index sur `LastEditedTime` pour les futures queries diff\u00e9rentielles.
- **`AppDbContext`** (`Dashboard.Data/Persistence/AppDbContext.cs`) : 5 `DbSet<T>`, `ApplyConfigurationsFromAssembly`.
- **Mappings bi-directionnels statiques** (`Dashboard.Data/Persistence/Mappings/`) : `TodoEntityMapping`, `JobApplicationEntityMapping`, `JournalEntryEntityMapping`, `HealthReadingEntityMapping` avec `ToDomain(entity)` + `CopyInto(item, lastEditedTime, target)`. Helpers internes `JsonListSerializer` (System.Text.Json) et `DateRangeMapping`.
- **Repositories EF** (`Dashboard.Data/Persistence/Repositories/`) : `TodoRepository`, `JobApplicationRepository`, `JournalEntryRepository`, `HealthReadingRepository`, `SyncCursorStore`. Upsert via `FindAsync` puis `Add` ou mutation + `SaveChanges`. `DeleteMissingAsync` supprime toute ligne absente de la derni\u00e8re sync.
- **Extension DI** : `PersistenceServiceCollectionExtensions.AddPersistence(services, connectionString)` enregistre `AppDbContext` (SQLite) + les 5 repositories en `Scoped`.
- **Tests** (`Dashboard.Data.Tests/Persistence/`) :
  - `SqliteInMemoryFixture` : connexion SQLite `:memory:` maintenue ouverte, `EnsureCreated()`.
  - `TodoRepositoryTests` (4) : insertion, update sur m\u00eame ID, round-trip complet (tags, deux `DateRange`, listes de strings), `DeleteMissingAsync`, non-op si rien \u00e0 supprimer.
  - `JobApplicationRepositoryTests` (1) : round-trip des trois dates et des multi-select.
  - `JournalEntryRepositoryTests` (1) : round-trip.
  - `HealthReadingRepositoryTests` (1) : round-trip des 25 m\u00e9triques + `ExerciseType`.
  - `SyncCursorStoreTests` (2) : `GetAsync` null si absent, `UpsertAsync` ins\u00e8re puis met \u00e0 jour.
  - `PersistenceServiceCollectionExtensionsTests` (1) : v\u00e9rifie que tous les services r\u00e9solvent depuis un scope DI.

### Choix assum\u00e9s

1. **Cache de Notion + curseurs de sync** (vs source de v\u00e9rit\u00e9 autonome). Notion reste la v\u00e9rit\u00e9 ; la base locale est un cache align\u00e9 par sync diff\u00e9rentielle au Lot 7. Motif\u00a0: mono-lecteur, pas de gestion de conflits bidirectionnels. D\u00e9taill\u00e9 dans ADR-0006.
2. **Entit\u00e9s EF s\u00e9par\u00e9es** (vs mapping direct sur les records). Les records du domaine restent immuables et framework-free (ADR-0005 pr\u00e9serv\u00e9). Coût\u00a0: ~200 lignes de mapping, acceptable pour la clart\u00e9 architecturale.
3. **Collections en JSON dans des colonnes TEXT** (vs tables jointes normalis\u00e9es). Pour une volum\u00e9trie de quelques centaines de lignes par data source, le filtrage LINQ-to-Objects est suffisant et \u00e9vite 8 tables de jointure. D\u00e9taill\u00e9 dans ADR-0006.
4. **`DateRange` persist\u00e9 en trois colonnes plates** (vs JSON). Permet de trier/filtrer SQL sur les dates sans d\u00e9s\u00e9rialiser. Convention\u00a0: les deux colonnes `Start`/`End` nulles \u2194 DateRange absent au niveau domaine.
5. **Pas de migration EF g\u00e9n\u00e9r\u00e9e \u00e0 ce lot**. L'environnement de d\u00e9veloppement cloud ne dispose pas de l'outillage `dotnet ef`. Les tests utilisent `EnsureCreated()` sur SQLite in-memory. `dotnet ef migrations add Initial` \u00e0 g\u00e9n\u00e9rer localement par Antoine avant le Lot 7 (premier write r\u00e9el sur device). D\u00e9tail dans ADR-0006.
6. **`DeleteMissingAsync` charge la liste obsol\u00e8te en m\u00e9moire** avant `RemoveRange` plut\u00f4t que d'utiliser `ExecuteDeleteAsync`. Motif\u00a0: volum\u00e9trie faible, tracking d\u00e9j\u00e0 d\u00e9sactiv\u00e9 via la logique, et `ExecuteDeleteAsync` ne d\u00e9clenche pas certaines interceptions \u2014 sacrifice mineur.
7. **Repositories `Scoped`** : coh\u00e9rent avec la lifetime d\u00e9faut d'EF Core. L'orchestrateur de sync (Lot 7) utilisera un scope explicite.
8. **`SyncCursor` expos\u00e9 en `.Core`** c\u00f4t\u00e9 domaine (pas que c\u00f4t\u00e9 entity). Motif\u00a0: le Lot 7 manipule ce record comme une valeur m\u00e9tier (« j'ai vu Notion jusqu'\u00e0 tel instant »), pas juste comme un DTO persist\u00e9.

### V\u00e9rifications

- Build et tests \u00e0 valider via la CI GitHub Actions (le sandbox ne dispose pas du SDK `dotnet`).
- Aucun secret litt\u00e9ral introduit.
- Entit\u00e9s EF priv\u00e9es de r\u00e9f\u00e9rence Notion\u00a0: la couche persistance ignore l'existence de Notion (cloison nette).

### Ce qui reste \u00e0 faire au prochain lot (Lot 7)

Termin\u00e9, voir section ci-dessous.

## Lot 7 \u2014 Sync diff\u00e9rentielle Notion (2026-04-19)

### Livrables

- **Snapshot Notion** (`Dashboard.Core/Notion/NotionSnapshot.cs`) : record `NotionSnapshot<T>(T Item, DateTimeOffset LastEditedTime)` qui transporte le `last_edited_time` aux c\u00f4t\u00e9s de l'item de domaine, n\u00e9cessaire pour `UpsertAsync(item, lastEditedTime)`.
- **Interface du service Notion** (`Dashboard.Core/Notion/INotionService.cs`) : 4 m\u00e9thodes `IAsyncEnumerable<NotionSnapshot<T>> Get*Async(DateTimeOffset? editedOnOrAfter = null, CancellationToken ct = default)`. `NotionService` (Data) impl\u00e9mente cette interface ; mod\u00e8le test\u00e9 par mocking dans le Lot 7.
- **Filtre `last_edited_time` server-side** (`Dashboard.Data/Notion/NotionApiClient.cs`) : `QueryDataSourceOneBatchAsync` et `QueryDataSourceAsync` acceptent un `editedOnOrAfter` optionnel, traduit en `filter.timestamp = "last_edited_time"` + `filter.last_edited_time.on_or_after = "yyyy-MM-ddTHH:mm:ss.fffZ"` (UTC). Aucun filtre \u00e9mis quand le param\u00e8tre est `null` (back-compat avec les tests existants).
- **Rapport de sync** (`Dashboard.Core/Abstractions/SyncReport.cs`) : `SyncSourceResult(DataSourceId, Success, WasFullSync, Upserts, Deletes, ErrorMessage)` avec factories `Ok`/`Failure` ; `SyncReport(IReadOnlyList<SyncSourceResult>)` avec `AllSucceeded`.
- **Interface orchestrateur** (`Dashboard.Core/Abstractions/ISyncOrchestrator.cs`) : `Task<SyncReport> SyncAllAsync(CancellationToken ct = default)`.
- **Orchestrateur** (`Dashboard.Data/Sync/SyncOrchestrator.cs`) : g\u00e9n\u00e9rique `SyncOneAsync<T>` param\u00e9tr\u00e9 par fetch + upsert + deleteMissing + getId. Strat\u00e9gie par source :
  - Pas de curseur ou `LastSyncCompleted` plus ancien que **6 h** \u2192 full sync (sans filtre Notion) puis `DeleteMissingAsync(seenIds)` pour aligner les suppressions ; on \u00e9crit `LastSyncCompleted = now` dans le curseur.
  - Sinon \u2192 delta sync avec `editedOnOrAfter = cursor.LastEditedSeen` ; pas de `DeleteMissing` ; on conserve l'ancien `LastSyncCompleted`.
  - `LastEditedSeen` est mis \u00e0 jour vers le max observ\u00e9 lors de l'it\u00e9ration.
  - Try/catch isol\u00e9 par \u00e9tape (lecture curseur, fetch+upsert, `DeleteMissing`, upsert curseur). Une exception (sauf `OperationCanceledException`) loggue un warning et renvoie `SyncSourceResult.Failure` pour cette source\u00a0; les 3 autres aboutissent.
- **`DeleteMissingAsync` retourne `Task<int>`** (vs `Task` au Lot 6) sur les 4 repositories\u00a0: la valeur alimente `SyncSourceResult.Deletes`.
- **DI** : `SyncServiceCollectionExtensions.AddSyncOrchestrator(services)` enregistre `ISyncOrchestrator \u2192 SyncOrchestrator` en `Scoped`. Les d\u00e9pendances (`INotionService`, repos, `ISyncCursorStore`, `IClock`, `IOptions<NotionOptions>`) sont fournies par les extensions des Lots pr\u00e9c\u00e9dents (`AddNotionClient`, `AddPersistence`).
- **Tests** :
  - `Dashboard.Data.Tests/Notion/NotionApiClientTests.cs` (3 tests ajout\u00e9s) : pas de `filter` quand `editedOnOrAfter` est `null` ; `filter.last_edited_time.on_or_after` au format ISO UTC quand fourni ; filter propag\u00e9 \u00e0 chaque batch lors de la pagination.
  - `Dashboard.Data.Tests/Sync/SyncOrchestratorTests.cs` (6 tests) : premier run = full sync sans filtre + `DeleteMissingAsync` appel\u00e9 + curseur \u00e9crit avec les deux dates ; delta dans la fen\u00eatre 6 h (filtre = `LastEditedSeen`, pas de delete, `LastSyncCompleted` inchang\u00e9) ; full sync apr\u00e8s 6 h ; isolation des erreurs (1 source qui jette n'arr\u00eate pas les 3 autres) ; `AllSucceeded == true` quand les 4 r\u00e9ussissent ; \u00e9chec lecture curseur \u2192 `Failure` sans appel \u00e0 Notion. Mocks Moq pour `INotionService`, repos, `ISyncCursorStore`, `IClock` ; helper `AsyncSeq<T>` pour les `IAsyncEnumerable`.

### Choix assum\u00e9s

1. **Filtre server-side `last_edited_time.on_or_after`** (vs filtrage client). Limite la bande passante et les pages \u00e0 mapper. La granularit\u00e9 milliseconde est suffisante (l'API Notion ne descend pas plus bas).
2. **Full sync toutes les 6 h** pour d\u00e9tecter les suppressions (vs polling d\u00e9di\u00e9 ou webhooks). 6 h est un compromis entre fra\u00eecheur de la d\u00e9tection et co\u00fbt de la full sync sur 4 data sources. Constante `internal static readonly` ajustable. Webhooks Notion non disponibles pour les integrations internes.
3. **Isolation par source via try/catch dans `SyncOneAsync`** (vs `Task.WhenAll` + `AggregateException`). L'orchestration s\u00e9quentielle (4 sources, faible volum\u00e9trie) simplifie le contr\u00f4le des transactions EF (DbContext scoped, partag\u00e9). Aucun gain attendu d'une parall\u00e9lisation.
4. **`NotionSnapshot<T>` introduit en Core** (vs ajouter `LastEditedTime` aux records de domaine). Les records m\u00e9tier doivent rester purs (ADR-0005) ; le `last_edited_time` est une information de transport Notion, pas du domaine.
5. **`DeleteMissingAsync` retourne le nombre supprim\u00e9** : permet d'alimenter `SyncReport.Deletes` sans recompter. R\u00e9trocompatibilit\u00e9 cass\u00e9e en interne uniquement (interfaces et 4 impls + tests mis \u00e0 jour).
6. **Curseur \u00e9crit m\u00eame quand 0 upsert** : permet de mettre \u00e0 jour `LastSyncCompleted` apr\u00e8s un full sync vide (sinon on relancerait un full sync au prochain run, gaspillage).
7. **`MockBehavior.Strict` sur `INotionService`** dans les tests pour d\u00e9tecter tout appel non pr\u00e9vu (param\u00e8tres `editedOnOrAfter` mal pass\u00e9s).
8. **Pas de retry au niveau de l'orchestrateur** : le `NotionApiClient` est d\u00e9j\u00e0 derri\u00e8re `AddStandardResilienceHandler` (Polly). Empiler un retry suppl\u00e9mentaire amplifierait inutilement la charge sur Notion.

### V\u00e9rifications

- Build et tests \u00e0 valider via la CI GitHub Actions (le sandbox ne dispose pas du SDK `dotnet`).
- Aucun secret litt\u00e9ral introduit ; les tests mockent l'`INotionService` enti\u00e8rement.
- Le format ISO du filtre (`yyyy-MM-ddTHH:mm:ss.fffZ`) est v\u00e9rifi\u00e9 par assertion sur le body s\u00e9rialis\u00e9.

### Ce qui reste \u00e0 faire au prochain lot (Lot 8)

Termin\u00e9, voir section ci-dessous.

## Lot 8 \u2014 Lecture CalendarContract (2026-04-19)

### Livrables

- **Domaine** (`Dashboard.Core/Domain/CalendarEvent.cs`) : record `sealed` (`Id`, `Title`, `Start`, `End`, `IsAllDay`, `CalendarId`, `CalendarDisplayName`). Bornes en `DateTimeOffset` UTC ; l'affichage local est de la responsabilit\u00e9 du widget.
- **Abstractions Calendar** (`Dashboard.Core/Abstractions/Calendar/`) :
  - POCOs `RawCalendarRow` (`CalendarId`, `DisplayName`, `AccountName`, `Color`, `Visible`) et `RawEventRow` (`EventId`, `CalendarId`, `Title`, `BeginUtcMillis`, `EndUtcMillis`, `IsAllDay`, `EventTimezone`).
  - `ICalendarContentReader` : `ReadCalendars()` (`Visible = 1`) + `ReadInstances(from, to)` \u2014 niveau d'abstraction valid\u00e9 par ADR-0004.
  - `ICalendarPermissionRequester` : `IsGrantedAsync` (appel\u00e9 silencieusement par le service) + `RequestAsync` (d\u00e9clench\u00e9 explicitement par l'UI).
  - `ICalendarService` : `Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(from, to, ct)`.
- **Service m\u00e9tier** (`Dashboard.Core/Services/AndroidCalendarService.cs`) : pure C#, consomme `ICalendarContentReader` + `ICalendarPermissionRequester`. Permission refus\u00e9e \u2192 liste vide silencieuse. Mappe `BeginUtcMillis`/`EndUtcMillis` via `DateTimeOffset.FromUnixTimeMilliseconds`. Trie par `Start` ascendant. Joint le `DisplayName` du calendrier via dictionnaire ; `string.Empty` si calendrier inconnu.
- **Impl\u00e9mentations Android** (`Dashboard.App/Platforms/Android/Services/`) :
  - `DefaultCalendarContentReader` (`internal sealed`) : ContentResolver \u2192 ICursor avec projections explicites sur `CalendarContract.Calendars` (`_id`, `CALENDAR_DISPLAY_NAME`, `ACCOUNT_NAME`, `CALENDAR_COLOR`, `VISIBLE`) et `CalendarContract.Instances` (`EVENT_ID`, `CALENDAR_ID`, `TITLE`, `BEGIN`, `END`, `ALL_DAY`, `EVENT_TIMEZONE`). URI `Instances` construite via `ContentUris.AppendId(begin)` + `AppendId(end)`.
  - `AndroidCalendarPermissionRequester` (`internal sealed`) : wrappe `Permissions.CheckStatusAsync<Permissions.CalendarRead>` et `Permissions.RequestAsync<Permissions.CalendarRead>` ; appels marshal\u00e9s sur le main thread via `MainThread.InvokeOnMainThreadAsync`.
- **DI + permissions** :
  - `Dashboard.Core/Services/CalendarServiceCollectionExtensions.AddCalendarService` : enregistre `ICalendarService` en `Singleton`.
  - `MauiProgram.cs` : `AddSingleton<ICalendarContentReader, DefaultCalendarContentReader>` + `AddSingleton<ICalendarPermissionRequester, AndroidCalendarPermissionRequester>` + `AddCalendarService()`.
  - `AndroidManifest.xml` : `<uses-permission android:name="android.permission.READ_CALENDAR" />`.
- **Tests** (`Dashboard.Core.Tests/Services/AndroidCalendarServiceTests.cs`, 6 tests, fake `ICalendarContentReader` en m\u00e9moire + `Mock<ICalendarPermissionRequester>` Moq) : permission refus\u00e9e \u2192 liste vide + reader jamais appel\u00e9 ; mapping complet d'une ligne ; tri par `Start` ascendant ; `IsAllDay` propag\u00e9 + titre `null` \u2192 `string.Empty` ; \u00e9v\u00e8nement orphelin (calendrier inconnu) \u2192 `CalendarDisplayName = string.Empty` ; fen\u00eatre temporelle (`from`, `to`) propag\u00e9e au reader.

### Choix assum\u00e9s

1. **Service nomm\u00e9 `AndroidCalendarService` mais plac\u00e9 en `.Core`** (vs `.App`). Coh\u00e9rent avec ROADMAP et ADR-0004 : la sp\u00e9cialisation \u00ab Android \u00bb d\u00e9signe la **source de donn\u00e9es** (ContentResolver via `CalendarContract`), pas la plateforme d'ex\u00e9cution. Le service est pur C#, sans d\u00e9pendance MAUI/Android.
2. **Service silencieux quand la permission est refus\u00e9e** (vs throw / r\u00e9sultat union). Permet aux widgets de d\u00e9grader leur affichage simplement (\u00ab vide \u00bb \u2261 \u00ab pas accord\u00e9 \u00bb \u2261 \u00ab rien dans la fen\u00eatre \u00bb). Le ViewModel/UI doit appeler `RequestAsync` explicitement (page Settings, premi\u00e8re utilisation) \u2014 le service ne demande jamais lui-m\u00eame.
3. **Filtrage \u00ab calendriers visibles uniquement \u00bb fait c\u00f4t\u00e9 reader** (`WHERE VISIBLE = 1`). \u00c9vite de remonter et filtrer ensuite des centaines de calendriers cach\u00e9s. Le filtrage \u00ab par toggle utilisateur \u00bb (pr\u00e9f\u00e9rences) viendra au Lot 13 (SettingsPage).
4. **Fen\u00eatre temporelle param\u00e9tr\u00e9e** (vs m\u00e9thodes `GetTodayAsync`/`GetWeekAsync`). Le ViewModel d\u00e9cide ; \u00e9vite la prolif\u00e9ration de surfaces API. Les helpers \u00ab semaine en cours \u00bb seront du code ViewModel, pas service.
5. **`CalendarContract.Instances` (et non `Events`)** \u2014 c'est une cons\u00e9quence d'ADR-0004 : on veut la liste expans\u00e9e par occurrence (r\u00e9currents inclus) sur la fen\u00eatre, pas les masters Events.
6. **Adaptateur `ICursor` non test\u00e9 unitairement** \u2014 conforme ADR-0004 : `DefaultCalendarContentReader` est intentionnellement trivial et serait test\u00e9 en int\u00e9gration sur device si besoin. La logique m\u00e9tier (mapping, tri, jointure DisplayName, fallback) est int\u00e9gralement test\u00e9e via `AndroidCalendarService`.
7. **Permission marshal\u00e9e sur le main thread** : `Permissions.RequestAsync` n\u00e9cessite l'UI thread sur Android (ouvre une boîte de dialogue syst\u00e8me). `IsGrantedAsync` y est aussi marshal\u00e9 par s\u00e9curit\u00e9 (ne d\u00e9clenche pas de dialog mais reste coh\u00e9rent).
8. **`Color` brut (`int` ARGB Android)** stock\u00e9 dans `RawCalendarRow` mais **non remont\u00e9** dans `CalendarEvent` \u00e0 ce lot. R\u00e9serve cette donn\u00e9e pour le Lot 11 si la palette daltonien-safe ne suffit pas et que le widget veut r\u00e9utiliser la couleur du calendrier (avec doublage ic\u00f4ne/label).

### V\u00e9rifications

- Build et tests \u00e0 valider via la CI GitHub Actions (le sandbox ne dispose pas du SDK `dotnet`).
- Aucun secret introduit ; les impls Android touchent `ContentResolver` et `Permissions`, jamais d'identifiant ou token.
- Aucune r\u00e9f\u00e9rence \u00e0 Google Calendar API : la lecture est 100 % locale (CLAUDE.md).

### Ce qui reste \u00e0 faire au prochain lot (Lot 9)

- `CrossDomainInsightsService` (4 r\u00e8gles d\u00e9terministes \u00e0 cadrer avec Antoine) + tests unitaires.
- Logger les valeurs d'enums Notion inconnues rencontr\u00e9es lors des syncs (report\u00e9 du Lot 5/7, \u00e0 grouper avec un futur lot t\u00e9l\u00e9m\u00e9trie).

# Architecture

## Principes directeurs

1. **Lecture seule** : aucune \u00e9criture dans Notion ni dans le calendrier Android.
2. **Offline-first raisonn\u00e9** : Notion est cach\u00e9 en SQLite (sync diff\u00e9rentielle), le calendrier n'est jamais cach\u00e9 (lecture directe via `CalendarContract`).
3. **Aucun backend, aucun OAuth** : un token Notion personnel saisi par l'utilisateur, le calendrier via le Content Provider Android natif.
4. **Accessibilit\u00e9 daltonien-first** : palette rouge/orange/bleu + neutres, ic\u00f4nes syst\u00e9matiques.
5. **Testabilit\u00e9** : `.Core` et `.Data` en `net10.0` pur (pas de workload requis pour tester).

## D\u00e9coupage

### `Dashboard.Core` (net10.0)

Domaine pur. **Aucune d\u00e9pendance plateforme ni infrastructure.**

- `Domain/` — records immuables : `TodoTask`, `JobApplication`, `HealthEntry`, `JournalEntry`, `CalendarInfo`, `CalendarEvent`.
- `Abstractions/` — `INotionService`, `ICalendarService`, `ITokenProvider`, `IClock`, `IInsightsService`.
- `Insights/` — `CrossDomainInsightsService` (r\u00e8gles d\u00e9terministes, aucun LLM).
- `Common/` — types transverses (`Result<T>`, `SyncCursor`).

### `Dashboard.Data` (net10.0)

Infrastructure d'acc\u00e8s aux donn\u00e9es.

- `Notion/` — `NotionApiClient` (HttpClient typ\u00e9, pagination, retry Polly), DTOs, mappers vers domaine.
- `Persistence/` — `AppDbContext` (EF Core 10 + Sqlite), repositories, migrations.
- `Sync/` — `SyncOrchestrator` avec strat\u00e9gie `last_edited_time`.

### `Dashboard.App` (net10.0-android)

UI MAUI et plateforme Android.

- `MauiProgram.cs` — bootstrap DI, configuration, Serilog, user-secrets `#if DEBUG`.
- `Views/`, `ViewModels/`, `Controls/`, `Converters/`, `Resources/Styles/`.
- `Platforms/Android/Services/AndroidCalendarService.cs` — impl\u00e9mente `ICalendarService` via `CalendarContract`.
- `Platforms/Android/Services/SecureStorageTokenProvider.cs` — impl\u00e9mente `ITokenProvider`.

### Tests

- `Dashboard.Core.Tests` — domaine et insights.
- `Dashboard.Data.Tests` — `NotionApiClient` (via `HttpMessageHandler` mock\u00e9), EF Core (SQLite in-memory), `SyncOrchestrator`.
- `Dashboard.App.Tests` — logique ViewModel framework-free.

## Strat\u00e9gie de sync

```
           \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510       \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
           \u2502 DashboardVM       \u2502       \u2502 SettingsVM         \u2502
           \u2514\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518       \u2514\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
               \u2502                              \u2502
        \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
        \u2502                                        \u2502
  \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510                   \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
  \u2502 INotionService  \u2502                   \u2502 ICalendarService    \u2502
  \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518                   \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
         \u2502                                    \u2502
  \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510             \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
  \u2502 SyncOrchestrator  \u2502             \u2502 AndroidCalendarService  \u2502
  \u2514\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2518             \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
        \u2502        \u2502                         \u2502
  \u250c\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2510 \u250c\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2510   \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
  \u2502 Notion  \u2502 \u2502 AppDb   \u2502   \u2502 ICalendarContentReader \u2502
  \u2502 API     \u2502 \u2502 Context \u2502   \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
  \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518 \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518           \u2502
                                \u250c\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
                                \u2502 ContentResolver   \u2502
                                \u2502 (CalendarContract)\u2502
                                \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
```

### Sync Notion (sur 4 data sources)

1. Au d\u00e9marrage, au pull-to-refresh, et toutes les 30 min (timer configurable) :
2. `SyncOrchestrator` lit le `SyncCursor` local par data source (dernier `last_edited_time` connu).
3. Appelle `POST /v1/data_sources/{id}/query` avec `filter.timestamp: last_edited_time >= cursor`.
4. Pagine via `next_cursor` jusqu'\u00e0 \u00e9puisement.
5. Upsert des entit\u00e9s en base locale ; persiste le nouveau `SyncCursor`.
6. Concurrence born\u00e9e (max 2 bases en parall\u00e8le pour respecter les rate limits Notion).

### Calendrier

Lecture directe \u00e0 la vol\u00e9e via `AndroidCalendarService.GetEventsAsync(from, to)`. Aucun cache : `CalendarContract.Instances` est d\u00e9j\u00e0 un index local g\u00e9r\u00e9 par Android.

## Gestion des secrets

| Environnement | Source | Lecture |
|---|---|---|
| Dev (machine) | `dotnet user-secrets` | `IConfiguration` (si `#if DEBUG`) |
| Prod (device) | `SecureStorage.Default` | `ITokenProvider` via impl\u00e9mentation Android |

Flux r\u00e9solution :
1. `ITokenProvider.GetNotionTokenAsync()` appelle `SecureStorage.Default.GetAsync("notion_token")`.
2. Si `null` et build Debug : fallback `IConfiguration["Notion:IntegrationToken"]`.
3. Si toujours `null` : l'UI force l'onboarding via `SettingsPage`.

**Aucun token n'est jamais log\u00e9 ni expos\u00e9 par `ToString()` / s\u00e9rialisation.**

## Pattern platform-specific (`ICalendarService`)

- Interface dans `.Core`, agnostique plateforme.
- Impl\u00e9mentation Android dans `.App/Platforms/Android/Services/AndroidCalendarService.cs`.
- Abstraction interne `ICalendarContentReader` (Option B retenue, voir ADR-0004) : retourne des `RawCalendarRow`/`RawEventRow` POCOs, permettant des tests sans \u00e9mulateur.
- Enregistrement DI conditionnel :
  ```csharp
  #if ANDROID
  builder.Services.AddSingleton<ICalendarContentReader, DefaultCalendarContentReader>();
  builder.Services.AddSingleton<ICalendarService, AndroidCalendarService>();
  #else
  builder.Services.AddSingleton<ICalendarService, UnsupportedPlatformCalendarService>();
  #endif
  ```

## Logging

- Serilog, sink fichier local (dans `FileSystem.AppDataDirectory` sur Android) + sink Debug.
- **Jamais** d'\u00e9criture de tokens, d'en-t\u00eates d'autorisation, ni de payloads bruts Notion dans les logs.
- Enrichissement : `SourceContext`, `ThreadId`, timestamp UTC.

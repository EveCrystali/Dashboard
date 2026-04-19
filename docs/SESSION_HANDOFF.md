# Prompt d'export \u2014 d\u00e9marrage d'une nouvelle session Claude

Ce document contient le prompt \u00e0 coller dans une **nouvelle session Claude Code** (y compris cloud, sans acc\u00e8s \u00e0 la m\u00e9moire locale d'une session pr\u00e9c\u00e9dente) pour reprendre le travail sur le projet Dashboard sans perdre le contexte.

Il est **auto-suffisant** : il embarque le profil utilisateur, les r\u00e8gles de collaboration, les choix techniques valid\u00e9s, l'avancement actuel et les prochaines \u00e9tapes.

---

## \u2702\ufe0f \u2014\u2014\u2014 Copier-coller tout ce qui suit dans la nouvelle session \u2014\u2014\u2014 \u2702\ufe0f

Tu rejoins le projet **Dashboard** en cours de d\u00e9veloppement. Lis int\u00e9gralement ce prompt avant toute action. \u00c0 la fin tu exposeras en 5-10 lignes ta compr\u00e9hension du contexte et tu demanderas confirmation avant de d\u00e9marrer quoi que ce soit.

## 1. Qui est l'utilisateur

**Antoine**, d\u00e9veloppeur C# .NET en reconversion professionnelle. Double r\u00f4le du projet :
1. Usage personnel quotidien (priorit\u00e9 #1).
2. Portfolio pour ses candidatures \u2014 code propre, test\u00e9, architectur\u00e9, document\u00e9.

**Accessibilit\u00e9 non-n\u00e9gociable** : Antoine est daltonien (deut\u00e9ranopie). Aucune UI ne doit coder l'information uniquement par distinction rouge/vert. Palette autoris\u00e9e : rouge + orange + bleu + neutres. Toute couleur doit \u00eatre doubl\u00e9e d'une ic\u00f4ne ou d'un label texte.

## 2. R\u00e8gles de collaboration

- **Style direct**, pas de flatterie, pas de pr\u00e9ambule.
- **Qualification \u00e9pist\u00e9mique** : Certain / Probable / Hypoth\u00e8se sur les affirmations non triviales.
- Sur tout choix technique non trivial, **donner l'alternative** et justifier le parti pris.
- Si une d\u00e9cision rel\u00e8ve d'Antoine (ex. choix de biblioth\u00e8que, strat\u00e9gie de test), **formuler la question explicitement** au lieu de trancher silencieusement.
- **Commits** en fran\u00e7ais, Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`).

## 3. R\u00e8gles strictes de s\u00e9curit\u00e9

**Tu n'auras JAMAIS acc\u00e8s aux valeurs r\u00e9elles des secrets** (token Notion, cl\u00e9s API). Antoine ne les fournira sous aucun pr\u00e9texte.

- Interdit : toute cha\u00eene litt\u00e9rale ressemblant \u00e0 un token m\u00eame en placeholder (`"secret_xxxx"`, `"test_token_123"`). Utiliser `string.Empty` ou `null`.
- Interdit : demander la valeur d'un token pour "tester". Si un test d'int\u00e9gration est n\u00e9cessaire, le g\u00e9n\u00e9rer et laisser Antoine l'ex\u00e9cuter.
- Interdit : \u00e9crire ou sugg\u00e9rer un `appsettings.Development.json` contenant des secrets.
- Obligatoire : tout secret lu via `ITokenProvider` (SecureStorage prod + user-secrets dev `#if DEBUG`).
- Logs : jamais de token ni d'en-t\u00eate `Authorization` dans la sortie Serilog.

## 4. Nature du projet

Application mobile Android **Dashboard** en .NET MAUI 10. Agr\u00e9ge :
- Donn\u00e9es **Notion** (API REST, version `2022-06-28`+, authentification par integration token personnel).
- Agenda **Google Calendar** via `CalendarContract` Android natif \u2014 **pas d'API cloud, pas d'OAuth, pas de projet Google Cloud, pas de backend**.

\u00c9cran unique Dashboard, 4 widgets scrollables : Aujourd'hui (\u00e9v\u00e9nements calendrier + t\u00e2ches Notion), Sant\u00e9 (derni\u00e8re entr\u00e9e Health Monitor + sparkline 7j), Candidatures actives, Journal r\u00e9cent.

## 5. Stack technique (valid\u00e9e, non n\u00e9gociable sauf d\u00e9cision explicite d'Antoine)

| Domaine | Choix | R\u00e9f\u00e9rence |
|---|---|---|
| Runtime | .NET 10 LTS + C# 14 (LangVersion=14) | `global.json`, `Directory.Build.props` |
| UI | .NET MAUI 10 (`net10.0-android`, min API 26, target API 36.0) | `src/Dashboard.App/Dashboard.App.csproj` |
| Multi-TFM | `.Core`/`.Data` en `net10.0` pur, `.App` en `net10.0-android` | ADR-0005 |
| MVVM | CommunityToolkit.Mvvm (source generators) | |
| DI | DI MAUI natif (`MauiAppBuilder.Services`) | |
| Persistance | EF Core 10 + Sqlite | ADR-0001 |
| HTTP | HttpClient typ\u00e9 + Polly (retry/backoff) | |
| Logs | Serilog (sink fichier local dans `FileSystem.AppDataDirectory`) | |
| Mocking | Moq 4.20.x | ADR-0002 |
| Tests | xUnit + Moq + FluentAssertions | |
| Navigation | Shell avec TabBar masqu\u00e9 en phase 1 | ADR-0003 |
| Calendrier testable | `ICalendarContentReader` haut niveau (POCOs) | ADR-0004 |
| Versions NuGet | Centralized Package Management (`Directory.Packages.props`) | |
| CI | GitHub Actions, deux jobs (Ubuntu non-MAUI + Windows MAUI Android) | `.github/workflows/ci.yml` |

## 6. Sources de donn\u00e9es

### Notion

Data sources d\u00e9j\u00e0 partag\u00e9es avec l'int\u00e9gration :
- T\u00e2ches : `f5e0c493-1e75-4d8b-bcdd-f16f7c4c0ee5`
- Candidatures : `ddd8f621-4493-465a-80c4-657d461c04b0`
- Journal Second Brain : `6a4776d3-f5f2-4dab-96de-98d66e5f3782`
- Health Monitor : `ff58ea49-daca-41da-9546-adbd93db18d6`
- Profil permanent (phase 2+) : `3303410f-2af0-813c-9792-ffc92683a130`

**Seuils sant\u00e9 perso** : HRV <53 ms, FC repos >61 BPM, sommeil <71.

### Calendrier Android

Lecture 100 % locale via `CalendarContract.Instances` (g\u00e8re les r\u00e9currences nativement). Permission `READ_CALENDAR` + demande runtime.

## 7. Structure du d\u00e9p\u00f4t (\u00e0 explorer d\u00e8s l'arriv\u00e9e)

```
Dashboard.slnx
\u251c\u2500\u2500 CLAUDE.md                (contexte permanent \u2014 re-lire \u00e0 chaque session)
\u251c\u2500\u2500 README.md
\u251c\u2500\u2500 global.json
\u251c\u2500\u2500 Directory.Build.props
\u251c\u2500\u2500 Directory.Packages.props
\u251c\u2500\u2500 docs/
\u2502   \u251c\u2500\u2500 ARCHITECTURE.md       (vue d'ensemble)
\u2502   \u251c\u2500\u2500 ROADMAP.md            (15 lots impos\u00e9s dans l'ordre)
\u2502   \u251c\u2500\u2500 PROGRESS.md           (\u00e9tat d'avancement \u00e0 jour)
\u2502   \u251c\u2500\u2500 SESSION_HANDOFF.md    (ce fichier)
\u2502   \u2514\u2500\u2500 ADRs/                 (d\u00e9cisions techniques)
\u251c\u2500\u2500 src/
\u2502   \u251c\u2500\u2500 Dashboard.Core/
\u2502   \u251c\u2500\u2500 Dashboard.Data/
\u2502   \u2514\u2500\u2500 Dashboard.App/
\u2514\u2500\u2500 tests/
    \u251c\u2500\u2500 Dashboard.Core.Tests/
    \u251c\u2500\u2500 Dashboard.Data.Tests/
    \u2514\u2500\u2500 Dashboard.App.Tests/
```

## 8. Avancement actuel

**Lot 1 (scaffolding) termin\u00e9 le 2026-04-19.**

Le build et les tests passent :
- `dotnet build -c Release` \u2192 0 warning, 0 erreur sur `.Core`, `.Data` et tests.
- `dotnet test` \u2192 3/3 tests de fum\u00e9e passent.
- `dotnet build src/Dashboard.App -c Debug -f net10.0-android` \u2192 0 warning, 0 erreur.

Voir [`docs/PROGRESS.md`](PROGRESS.md) pour le d\u00e9tail lot par lot et les \u00e9carts assum\u00e9s.

## 9. Prochaine \u00e9tape impos\u00e9e : Lot 2 \u2014 Gestion des secrets

Crit\u00e8res d'acceptation :
1. Interface `ITokenProvider` dans `Dashboard.Core/Abstractions/` :
   ```csharp
   public interface ITokenProvider
   {
       Task<string?> GetNotionTokenAsync(CancellationToken ct = default);
       Task SetNotionTokenAsync(string token, CancellationToken ct = default);
       Task ClearAsync(CancellationToken ct = default);
   }
   ```
2. Impl\u00e9mentation `SecureStorageTokenProvider` dans `Dashboard.App/Platforms/Android/Services/`.
3. Abstraction interne testable `ISecureStorageWrapper` pour d\u00e9coupler `SecureStorage.Default`.
4. Fallback `#if DEBUG` vers `IConfiguration["Notion:IntegrationToken"]` si SecureStorage vide.
5. `dotnet user-secrets init` sur `Dashboard.App`, branch\u00e9 dans `MauiProgram.cs` avec `builder.Configuration.AddUserSecrets<App>()` conditionn\u00e9 `#if DEBUG`.
6. Tests unitaires dans `Dashboard.App.Tests` (ou `Dashboard.Core.Tests`) couvrant :
   - Retour token SecureStorage quand pr\u00e9sent.
   - Fallback vers `IConfiguration` en Debug quand SecureStorage vide.
   - `null` retourn\u00e9 en Release quand SecureStorage vide.
   - `SetAsync` persiste correctement.
   - `ClearAsync` supprime le token.
7. Aucune cha\u00eene litt\u00e9rale ressemblant \u00e0 un token, m\u00eame en placeholder.
8. Commit final : `feat: gestion des secrets`.

## 10. Ordre impos\u00e9 (rappel)

Ne pas sauter de lot sans validation explicite d'Antoine. S\u00e9quence compl\u00e8te dans [`docs/ROADMAP.md`](ROADMAP.md).

## 11. Protocole de prise en main

1. Lis `CLAUDE.md`, `docs/ROADMAP.md`, `docs/PROGRESS.md` et les ADRs existants.
2. Lance `dotnet build` pour v\u00e9rifier l'\u00e9tat du d\u00e9p\u00f4t.
3. Expose en 5-10 lignes ta compr\u00e9hension du contexte et du prochain lot.
4. Demande confirmation \u00e0 Antoine avant de commencer \u00e0 coder.

\u2702\ufe0f \u2014\u2014\u2014 Fin du prompt \u00e0 copier \u2014\u2014\u2014 \u2702\ufe0f

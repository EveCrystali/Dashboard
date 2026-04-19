# ADR-0006 — Persistance locale : cache Notion avec entités EF séparées

- **Statut** : accepté
- **Date** : 2026-04-19
- **Décideur** : Antoine

## Contexte

ADR-0001 a tranché le choix technique (EF Core 10 + Sqlite). Le lot 6 précise
**le rôle** de la base locale, **la forme** des entités persistées et **la
sérialisation** des collections.

Trois questions ouvertes :

1. **Rôle de la base** : source de vérité autonome ou simple cache de Notion ?
2. **Forme des entités** : réutiliser les records du domaine (`TodoItem`,
   `JobApplication`…) directement via EF Core, ou définir des entités EF
   séparées mappées explicitement vers les records ?
3. **Collections** (tags, ids de relations, fichiers) : tables jointes normalisées
   ou sérialisées en JSON dans une colonne TEXT ?

## Décision

1. **Cache de Notion + table de curseurs de sync.** Chaque entité expose
   `LastEditedTime` (provenant de Notion) et la table `SyncCursors` stocke,
   par data source, le dernier curseur et la date de dernière synchronisation
   complète. Notion reste la source de vérité ; la base locale permet
   l'affichage hors ligne et sera alimentée en différentiel par le lot 7.
2. **Entités EF séparées, mapping explicite.** Les records de `Dashboard.Core.Domain`
   restent des DTOs immuables, orientés domaine. Les entités EF dans
   `Dashboard.Data.Persistence.Entities` sont des classes plates, avec
   propriétés settables, ctor sans paramètre et clés EF-friendly. La
   traduction entité↔record se fait via mappers statiques dans les
   repositories.
3. **Collections en JSON dans des colonnes TEXT.** `Tags`, `AssigneeIds`,
   `SubtaskUrls`, `Domains`, `Positions`, `CompanyTypes`, `ContactMethods`,
   `CvFileIds`, `CoverLetterFileIds`, `ExerciseTypes` sont sérialisées via
   `HasConversion` + `System.Text.Json`.

## Justification

### 1. Cache vs source de vérité

Les données vivent dans Notion, éditées sur téléphone/desktop via l'app
Notion officielle. Traiter la base locale comme source de vérité imposerait
de gérer des conflits bidirectionnels, hors scope pour une app mono-lecteur.
Le rôle de cache simplifie drastiquement la sync (delta via
`last_edited_time`) et permet un affichage immédiat au démarrage même sans
réseau.

La table `SyncCursors` prépare le lot 7 : elle mémorise, par data source,
la date du dernier `last_edited_time` vu, pour ne requêter que les pages
modifiées depuis.

### 2. Entités EF séparées

Alternative A : mapper EF directement sur les records (`TodoItem`, etc.).
Rejeté : les records avec init-only properties, enums, `IReadOnlyList<T>` et
value objects (`DateRange`) cohabitent mal avec le change tracking d'EF,
surtout en présence de conversions JSON. La couche domaine serait
contaminée par des attributs EF ou forcée à assouplir son immutabilité.

Alternative B retenue : entités EF plates, settables, avec mapping explicite.
Gain :

- Les records du domaine restent immuables et framework-free (ADR-0005).
- Les migrations EF n'affectent que `.Data`.
- Le mapping explicite rend visible la frontière entre « forme persistée »
  et « forme métier ».

Coût : duplication structurelle (~200 lignes de mapping), acceptable pour la
clarté architecturale et le signal portfolio.

### 3. JSON pour les collections

Alternative A : tables jointes normalisées (`TodoTags`, `JobApplicationPositions`,
etc.). Rejeté : ~8 tables de jointure pour un cache mono-lecteur, aucun
usage LINQ côté `WHERE tag IN (...)` prévu (les filtres se font côté liste
matérialisée en mémoire vu la volumétrie — quelques centaines de lignes
max).

Alternative B retenue : sérialisation JSON via `ValueConverter`. Gain :

- Une seule ligne par tâche/candidature, pas de `ThenInclude`.
- Rond-trip trivial, testable sans DB.
- Conservation de l'ordre de la source Notion.

Coût : pas de requête SQL directe sur un tag. Pour la volumétrie ciblée
(centaines de lignes), le filtrage en mémoire LINQ-to-Objects est
suffisant et plus simple que des jointures.

## Conséquences

- Nouveau namespace `Dashboard.Data.Persistence` contenant :
  - `Entities/` : `TodoEntity`, `JobApplicationEntity`, `JournalEntryEntity`,
    `HealthReadingEntity`, `SyncCursorEntity`.
  - `Configurations/` : une classe `IEntityTypeConfiguration<T>` par entité,
    avec conversions JSON.
  - `AppDbContext` exposant un `DbSet<T>` par entité.
  - `Repositories/` : implémentations EF des interfaces de `.Core.Abstractions`.
- Interfaces repository (`ITodoRepository`, `IJobApplicationRepository`,
  `IJournalEntryRepository`, `IHealthReadingRepository`, `ISyncCursorStore`)
  dans `Dashboard.Core/Abstractions/`. Signature cache : `UpsertAsync`,
  `GetAllAsync`, `DeleteMissingAsync(ids présents dans la dernière sync)`.
- Les mappings bi-directionnels vivent à côté des repositories sous forme
  de classes statiques `TodoEntityMapping`, etc., testables séparément.
- `DateRange` est persisté en trois colonnes (`Start`, `End`, `IsDateTime`)
  plutôt que JSON : trois champs plats permettent de trier sur la date sans
  désérialiser.
- `AddPersistence(IServiceCollection, connectionString)` expose le DbContext
  via `AddDbContext<AppDbContext>` et les repositories en `Scoped`.
- Migration initiale **différée** au premier vrai lancement sur device :
  Antoine exécutera `dotnet ef migrations add Initial` localement avant
  Lot 7. En attendant, les tests utilisent `EnsureCreated()` sur base
  in-memory SQLite.

## Dette assumée

- Pas de migration EF versionnée au lot 6 (à générer au plus tard avant le
  lot 7 qui écrit réellement en base sur device).
- Pas d'index explicites au-delà des clés primaires : à ajouter si les
  listes dépassent quelques milliers de lignes (très improbable pour un
  usage personnel).

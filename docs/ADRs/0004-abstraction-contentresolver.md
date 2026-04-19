# ADR-0004 — Abstraction du ContentResolver pour tests

- **Statut** : accept\u00e9
- **Date** : 2026-04-19
- **D\u00e9cideur** : Antoine

## Contexte

`AndroidCalendarService` doit lire le `ContentResolver` Android pour acc\u00e9der \u00e0 `CalendarContract.Calendars` et `CalendarContract.Instances`. Cette API utilise des `ICursor` de type colonnes-index, non-typ\u00e9s. La tester unitairement sans \u00e9mulateur demande une abstraction.

Deux niveaux possibles :

- **Option A** : interface `IContentResolverProvider` exposant directement `ICursor`. Tests fid\u00e8les \u00e0 l'API Android mais n\u00e9cessitent de mocker `ICursor` enti\u00e8rement (moveToNext, getColumnIndex, getString, getLong, getInt...) pour chaque test. Verbeux, cassant.
- **Option B** : interface `ICalendarContentReader` de plus haut niveau qui retourne `IEnumerable<RawCalendarRow>` et `IEnumerable<RawEventRow>` (POCOs). Les tests cr\u00e9ent directement les POCOs ; la logique de conversion `ICursor` \u2192 POCO n'est pas test\u00e9e unitairement mais devient un adaptateur fin et d\u00e9terministe.

## D\u00e9cision

**Option B.**

## Justification

- On teste la **logique m\u00e9tier** (mapping `RawEventRow` \u2192 domaine `CalendarEvent`, filtrage par calendrier activ\u00e9, gestion des all-day, fusion des fen\u00eatres temporelles), pas l'API Android elle-m\u00eame.
- L'adaptateur `DefaultCalendarContentReader` reste trivial et peut \u00eatre test\u00e9 en int\u00e9gration (sur device) si n\u00e9cessaire.
- Tests purement sur `net10.0` sans aucune d\u00e9pendance Android.
- R\u00e9duction drastique du bruit de test : un `RawEventRow` est un record de 10 champs, un mock `ICursor` demanderait 50 lignes de setup par sc\u00e9nario.

## Cons\u00e9quences

- Deux types POCOs dans `.Core/Abstractions/Calendar/` : `RawCalendarRow`, `RawEventRow`.
- Interface `ICalendarContentReader` dans `.Core`, impl\u00e9mentation `DefaultCalendarContentReader` dans `.App/Platforms/Android/Services/`.
- `AndroidCalendarService` devient testable unitairement via un fake reader en m\u00e9moire.
- L'adaptateur ICursor n'est pas couvert par les tests unitaires : risque accept\u00e9, couverture via tests manuels sur device.

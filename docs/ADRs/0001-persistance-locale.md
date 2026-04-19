# ADR-0001 — Persistance locale : EF Core 10 + Sqlite

- **Statut** : accept\u00e9
- **Date** : 2026-04-19
- **D\u00e9cideur** : Antoine

## Contexte

L'application doit cacher localement les donn\u00e9es Notion (sync incr\u00e9mental via `last_edited_time`). Deux options pertinentes sur Android MAUI :

- `sqlite-net-pcl` : micro-ORM tr\u00e8s l\u00e9ger, API minimaliste, pas de migrations automatiques ni de LINQ complet.
- **EF Core 10 + Sqlite** : ORM complet, migrations, LINQ, `DbContext`, tracking, change detection.

## D\u00e9cision

**EF Core 10 + Sqlite.**

## Justification

- Le projet a un r\u00f4le portfolio. La candidature cible des postes C# .NET ; d\u00e9montrer la ma\u00eetrise d'EF Core (ORM le plus utilis\u00e9 dans l'\u00e9cosyst\u00e8me .NET en entreprise) est un signal fort.
- Migrations versionn\u00e9es = \u00e9volution de sch\u00e9ma propre sur le long terme (changement de sources Notion, ajout de champs).
- LINQ complet simplifie la logique de requ\u00eatage, en particulier pour les insights crois\u00e9s (phase 2).
- Co\u00fbt d'ajout ~2 MB sur le package Android : n\u00e9gligeable sur device moderne.
- Surco\u00fbt de d\u00e9marrage \u00e0 froid mesur\u00e9 \u00e0 quelques centaines de ms sur device m\u00e9dian : acceptable dans le cadre d'une app ouverte une fois le matin.

## Cons\u00e9quences

- Le projet `Dashboard.Data` d\u00e9pend de `Microsoft.EntityFrameworkCore.Sqlite`.
- Les tests `Dashboard.Data.Tests` utilisent `Microsoft.Data.Sqlite` avec une base `":memory:"` partag\u00e9e pour la dur\u00e9e du test (pattern `Open` + `KeepAlive`).
- Les entit\u00e9s EF vivent dans `.Data` (DTOs persist\u00e9s), s\u00e9par\u00e9es des records de domaine dans `.Core`.
- Les migrations sont g\u00e9n\u00e9r\u00e9es via `dotnet ef migrations add` \u2014 workflow document\u00e9 dans le README.

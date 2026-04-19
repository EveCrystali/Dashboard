# ADR-0005 — Multi-TFM : .Core et .Data en net10.0 pur

- **Statut** : accept\u00e9
- **Date** : 2026-04-19
- **D\u00e9cideur** : Antoine

## Contexte

L'application cible Android via MAUI. Deux strat\u00e9gies de ciblage possibles :

- **A** : tout en `net10.0-android` (un seul TFM). Simple mais force tous les projets \u00e0 d\u00e9pendre du workload MAUI.
- **B** : `.Core` et `.Data` en `net10.0` pur, `.App` seul en `net10.0-android` (+ `net10.0-windows10.*` en phase 2).

## D\u00e9cision

**Option B.**

## Justification

- Les tests xUnit tournent **sans workload MAUI ni \u00e9mulateur Android** : build plus rapide, CI plus simple (Ubuntu), t\u00e9l\u00e9chargements NuGet r\u00e9duits.
- S\u00e9paration nette des responsabilit\u00e9s : `.Core` et `.Data` ne **peuvent pas** r\u00e9f\u00e9rencer `Microsoft.Maui.*` (le compilateur l'emp\u00eache), for\u00e7ant une architecture propre.
- Phase 2 (Windows) : l'ajout d'une seconde plateforme ne touche que `.App.csproj`.
- Possibilit\u00e9 de r\u00e9utiliser `.Core` et `.Data` dans un ventuel outil CLI de diagnostic.

## Cons\u00e9quences

- `.Core` interdit toute d\u00e9pendance plateforme (pas de `Microsoft.Maui.Storage`, etc.).
- Les abstractions plateforme (`ITokenProvider`, `ICalendarService`) vivent dans `.Core`, les impl\u00e9mentations dans `.App/Platforms/Android/`.
- La CI build `.Core`/`.Data`/tests sur `ubuntu-latest` (rapide), build `.App` sur `windows-latest` avec workload MAUI.

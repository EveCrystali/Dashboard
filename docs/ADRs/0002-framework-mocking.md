# ADR-0002 — Framework de mocking : Moq

- **Statut** : accept\u00e9
- **Date** : 2026-04-19
- **D\u00e9cideur** : Antoine

## Contexte

Les tests unitaires n\u00e9cessitent un framework de mocking pour simuler `INotionService`, `ICalendarService`, `ITokenProvider`, `HttpMessageHandler`, etc.

## D\u00e9cision

**Moq 4.20.x.**

## Justification

- Standard historique de l'\u00e9cosyst\u00e8me .NET ; familiarit\u00e9 maximale pour les recruteurs techniques lors de la revue du code portfolio.
- Documentation et exemples abondants.
- La controverse SponsorLink (aout 2023) a \u00e9t\u00e9 r\u00e9solue depuis la version 4.20.70 : le code de t\u00e9l\u00e9m\u00e9trie a \u00e9t\u00e9 retir\u00e9.

## Alternative rejet\u00e9e : NSubstitute

- Syntaxe plus naturelle, plus lisible pour un d\u00e9butant (pas de `It.IsAny<T>()`, pas de `Setup().Returns()`).
- \u00c9cosyst\u00e8me plus restreint, moins de mat\u00e9riel p\u00e9dagogique.
- D\u00e9cision d\u00e9libr\u00e9ment align\u00e9e sur l'usage industriel majoritaire.

## Cons\u00e9quences

- Tous les projets de tests r\u00e9f\u00e9rencent `Moq`.
- Les scenarios avec `HttpMessageHandler` utilisent un `Mock<HttpMessageHandler>` avec `Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ...)`.

# ADR-0003 — Navigation MAUI : Shell

- **Statut** : accept\u00e9
- **Date** : 2026-04-19
- **D\u00e9cideur** : Antoine

## Contexte

MAUI propose deux mod\u00e8les de navigation principaux : `Shell` (routing par URI, drawer/tabbar int\u00e9gr\u00e9s) et `NavigationPage` (pile manuelle).

L'application MVP a un seul \u00e9cran principal (Dashboard) + un \u00e9cran Param\u00e8tres + un \u00e9cran D\u00e9tail d'item. Potentiellement extensible en phase 2/3.

## D\u00e9cision

**Shell**, avec TabBar masqu\u00e9 en phase 1, route par d\u00e9faut `///dashboard`.

## Justification

- Routing d\u00e9claratif par URI : `Shell.Current.GoToAsync("detail?id=abc")` plus clair que la manipulation de pile.
- Pr\u00e9pare l'extension phase 2/3 (nouveaux \u00e9crans crois\u00e9s) sans refactor de navigation.
- Support natif de la profondeur de navigation, back button, deep links.

## Alternative rejet\u00e9e : NavigationPage

- Plus contr\u00f4l, moins de magie, mais plus verbeux pour chaque cas simple.
- Overkill n\u00e9gatif : pour un app avec quelques \u00e9crans, Shell reste aussi simple et offre plus de marge.

## Cons\u00e9quences

- `AppShell.xaml` d\u00e9finit la structure de navigation ; routes enregistr\u00e9es dans `AppShell.xaml.cs` via `Routing.RegisterRoute`.
- TabBar masqu\u00e9 avec `Shell.TabBarIsVisible="False"` en phase 1 (un seul onglet logique).

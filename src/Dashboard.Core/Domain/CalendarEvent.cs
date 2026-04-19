namespace Dashboard.Core.Domain;

/// <summary>
/// Évènement de calendrier projeté depuis le content provider Android
/// (<c>CalendarContract.Instances</c>). Les bornes sont exprimées en
/// <see cref="DateTimeOffset"/> avec décalage UTC ; le widget UI est
/// responsable de la conversion en heure locale d'affichage.
/// </summary>
public sealed record CalendarEvent(
    string Id,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    bool IsAllDay,
    string CalendarId,
    string CalendarDisplayName);

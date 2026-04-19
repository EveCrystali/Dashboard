namespace Dashboard.Core.Domain;

public enum HealthEntryType
{
    Journee,
    Sommeil,
    Constantes,
    Poids,
    Analyse,
    BilanHebdo,
}

public enum HealthVerdict
{
    Normal,
    Attention,
    Alerte,
}

public enum HealthSource
{
    SamsungWatch,
    Tensiometre,
    Balance,
    Manuel,
    ClaudeAnalyse,
}

public enum ExerciseType
{
    Musculation,
    Running,
    Marche,
    Padel,
    VTT,
    Natation,
    Autre,
}

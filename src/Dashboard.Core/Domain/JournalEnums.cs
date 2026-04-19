namespace Dashboard.Core.Domain;

public enum JournalType
{
    Atome,
    Decision,
    Snapshot,
    Connexion,
}

public enum JournalDomain
{
    Emploi,
    Cassian,
    Sante,
    Infra,
    Admin,
    Transversal,
}

public enum JournalSource
{
    Claude,
    Manuel,
    Synchro,
}

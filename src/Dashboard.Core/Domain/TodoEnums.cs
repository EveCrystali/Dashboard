namespace Dashboard.Core.Domain;

// Renommé TodoStatus (vs TaskStatus du schéma) pour éviter la collision avec
// System.Threading.Tasks.TaskStatus importé via ImplicitUsings.
public enum TodoStatus
{
    DuJour,
    Tache,
    EnCours,
    Bloquee,
    EnAttente,
    Done,
    Annulee,
}

public enum TodoPriority
{
    Haute,
    Moyenne,
    Basse,
}

public enum TodoTag
{
    Parentalite,
    Perso,
    Cassian,
    Administratif,
    Logement,
    AJEI,
    Travail,
    Cadeaux,
    Voyage,
    Mariage,
    RechercheEmploi,
    Apprentissage,
    SouvenirFrancais,
    Openclassroom,
}

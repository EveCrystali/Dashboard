namespace Dashboard.Core.Domain;

public enum JobAppStatus
{
    PretAPostuler,
    CandidatureSpontanee,
    ContacteParRecruteur,
    Recommande,
    CandidatureEnvoyee,
    Suivi,
    OffreFermeeSansRetour,
    ReponseNegativePerso,
    ReponseNegativeRecruteur,
    Signe,
    OffreRecue,
    Archive,
}

public enum JobPosition
{
    Geometre,
    Inconnu,
    DevCSharpNet,
    DevJava,
    TechSupportApplicatif,
    DevWeb,
    DevFullStackCSharpAngular,
    DevIt,
    DevOps,
    DevLogiciel,
}

public enum CompanyType
{
    ESN,
    EntrepriseInformatique,
    EntrepriseDeveloppeurs,
    EntrepriseRecrutementIt,
    EntrepriseIndustrielle,
    EntrepriseIngenierie,
    Autre,
}

public enum JobInterest
{
    Haute,
    Moyen,
    Bas,
}

public enum ContactMethod
{
    LinkedIn,
    Indeed,
    JobPostingPro,
    HelloWork,
    Site,
    CandidatureSpontanee,
    Mail,
    APEC,
    FranceTravail,
    JobsSmartrecruiters,
    Telephone,
    Meteojob,
    EJob,
}

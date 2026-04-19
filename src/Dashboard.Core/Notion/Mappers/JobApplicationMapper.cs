using Dashboard.Core.Domain;

namespace Dashboard.Core.Notion.Mappers;

/// <summary>
/// Mapping <see cref="NotionPage"/> → <see cref="JobApplication"/> pour la base
/// « Candidatures » (data source <c>ddd8f621-4493-465a-80c4-657d461c04b0</c>).
/// </summary>
/// <remarks>
/// Attention : « Date d'échéance » utilise une apostrophe droite U+0027 (contrairement
/// à la base Tâches). « Etat » est sans accent. « Résumé généré par IA » n'a pas
/// d'apostrophe. « Contacté par Recruteur » comporte un espace final dans Notion.
/// </remarks>
public static class JobApplicationMapper
{
    public const string ColumnCompany = "Entreprise";
    public const string ColumnStatus = "Etat";
    public const string ColumnPositions = "Poste";
    public const string ColumnCompanyTypes = "Type";
    public const string ColumnInterest = "Int\u00e9r\u00eat";
    public const string ColumnContactMethods = "M\u00e9thode de contact";
    public const string ColumnContactNotes = "Contact";
    public const string ColumnContactDate = "Date de contact";
    public const string ColumnDueDate = "Date d'\u00e9ch\u00e9ance";
    public const string ColumnFollowUpDate = "Date de relance";
    public const string ColumnOfferUrl = "URL";
    public const string ColumnCv = "CV";
    public const string ColumnCoverLetter = "Lettre de motivation";
    public const string ColumnAiSummary = "R\u00e9sum\u00e9 g\u00e9n\u00e9r\u00e9 par IA";

    private static readonly IReadOnlyDictionary<string, JobAppStatus> StatusByNotionName = new Dictionary<string, JobAppStatus>(StringComparer.Ordinal)
    {
        ["Pr\u00eat \u00e0 postuler"] = JobAppStatus.PretAPostuler,
        ["Candidature spontan\u00e9e"] = JobAppStatus.CandidatureSpontanee,
        ["Contact\u00e9 par Recruteur "] = JobAppStatus.ContacteParRecruteur,
        ["Recommand\u00e9"] = JobAppStatus.Recommande,
        ["Candidature envoy\u00e9e"] = JobAppStatus.CandidatureEnvoyee,
        ["Suivi"] = JobAppStatus.Suivi,
        ["Offre ferm\u00e9e sans retour"] = JobAppStatus.OffreFermeeSansRetour,
        ["R\u00e9ponse n\u00e9gative personnelle"] = JobAppStatus.ReponseNegativePerso,
        ["R\u00e9ponse n\u00e9gative recruteur"] = JobAppStatus.ReponseNegativeRecruteur,
        ["Sign\u00e9"] = JobAppStatus.Signe,
        ["Offre re\u00e7ue"] = JobAppStatus.OffreRecue,
        ["Archiv\u00e9"] = JobAppStatus.Archive,
    };

    private static readonly IReadOnlyDictionary<string, JobPosition> PositionByNotionName = new Dictionary<string, JobPosition>(StringComparer.Ordinal)
    {
        ["G\u00e9om\u00e8tre"] = JobPosition.Geometre,
        ["Inconnu"] = JobPosition.Inconnu,
        ["Dev C#/.NET"] = JobPosition.DevCSharpNet,
        ["Dev Java"] = JobPosition.DevJava,
        ["Tech Support Applicatif"] = JobPosition.TechSupportApplicatif,
        ["Dev Web"] = JobPosition.DevWeb,
        ["Dev FullStack C#/Angular"] = JobPosition.DevFullStackCSharpAngular,
        ["Dev IT"] = JobPosition.DevIt,
        ["DevOps"] = JobPosition.DevOps,
        ["Dev Logiciel"] = JobPosition.DevLogiciel,
    };

    private static readonly IReadOnlyDictionary<string, CompanyType> CompanyTypeByNotionName = new Dictionary<string, CompanyType>(StringComparer.Ordinal)
    {
        ["ESN"] = CompanyType.ESN,
        ["Entreprise informatique"] = CompanyType.EntrepriseInformatique,
        ["Entreprise d\u00e9veloppeurs"] = CompanyType.EntrepriseDeveloppeurs,
        ["Entreprise recrutement IT"] = CompanyType.EntrepriseRecrutementIt,
        ["Entreprise industrielle"] = CompanyType.EntrepriseIndustrielle,
        ["Entreprise ing\u00e9nierie"] = CompanyType.EntrepriseIngenierie,
        ["Autre"] = CompanyType.Autre,
    };

    private static readonly IReadOnlyDictionary<string, JobInterest> InterestByNotionName = new Dictionary<string, JobInterest>(StringComparer.Ordinal)
    {
        ["Haute"] = JobInterest.Haute,
        ["Moyen"] = JobInterest.Moyen,
        ["Bas"] = JobInterest.Bas,
    };

    private static readonly IReadOnlyDictionary<string, ContactMethod> ContactMethodByNotionName = new Dictionary<string, ContactMethod>(StringComparer.Ordinal)
    {
        ["LinkedIn"] = ContactMethod.LinkedIn,
        ["Indeed"] = ContactMethod.Indeed,
        ["JobPostingPro"] = ContactMethod.JobPostingPro,
        ["HelloWork"] = ContactMethod.HelloWork,
        ["Site"] = ContactMethod.Site,
        ["Candidature spontan\u00e9e"] = ContactMethod.CandidatureSpontanee,
        ["Mail"] = ContactMethod.Mail,
        ["APEC"] = ContactMethod.APEC,
        ["France Travail"] = ContactMethod.FranceTravail,
        ["Jobs Smartrecruiters"] = ContactMethod.JobsSmartrecruiters,
        ["T\u00e9l\u00e9phone"] = ContactMethod.Telephone,
        ["Meteojob"] = ContactMethod.Meteojob,
        ["EJob"] = ContactMethod.EJob,
    };

    public static JobApplication Map(NotionPage page, INotionPropertyReader reader)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(reader);

        var p = page.Properties;

        return new JobApplication(
            Id: page.Id,
            Company: p.ReadTitle(ColumnCompany, reader) ?? string.Empty,
            Status: MapEnum(p.ReadStatus(ColumnStatus, reader), StatusByNotionName, JobAppStatus.PretAPostuler),
            Positions: MapMulti(p.ReadMultiSelect(ColumnPositions, reader), PositionByNotionName),
            CompanyTypes: MapMulti(p.ReadMultiSelect(ColumnCompanyTypes, reader), CompanyTypeByNotionName),
            Interest: MapEnumNullable(p.ReadSelect(ColumnInterest, reader), InterestByNotionName),
            ContactMethods: MapMulti(p.ReadMultiSelect(ColumnContactMethods, reader), ContactMethodByNotionName),
            ContactNotes: p.ReadRichText(ColumnContactNotes, reader),
            ContactDate: p.ReadDate(ColumnContactDate, reader),
            DueDate: p.ReadDate(ColumnDueDate, reader),
            FollowUpDate: p.ReadDate(ColumnFollowUpDate, reader),
            OfferUrl: p.ReadUrl(ColumnOfferUrl, reader),
            CvFileIds: p.ReadFiles(ColumnCv, reader),
            CoverLetterFileIds: p.ReadFiles(ColumnCoverLetter, reader),
            AiSummary: p.ReadRichText(ColumnAiSummary, reader));
    }

    private static TEnum MapEnum<TEnum>(string? name, IReadOnlyDictionary<string, TEnum> map, TEnum fallback)
        where TEnum : struct, Enum =>
        name is not null && map.TryGetValue(name, out var v) ? v : fallback;

    private static TEnum? MapEnumNullable<TEnum>(string? name, IReadOnlyDictionary<string, TEnum> map)
        where TEnum : struct, Enum =>
        name is not null && map.TryGetValue(name, out var v) ? v : null;

    private static IReadOnlyList<TEnum> MapMulti<TEnum>(IReadOnlyList<string> names, IReadOnlyDictionary<string, TEnum> map)
        where TEnum : struct, Enum
    {
        if (names.Count == 0)
        {
            return [];
        }

        var result = new List<TEnum>(names.Count);
        foreach (var n in names)
        {
            if (map.TryGetValue(n, out var v))
            {
                result.Add(v);
            }
        }

        return result;
    }
}

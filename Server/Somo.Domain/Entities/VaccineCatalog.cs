namespace Somo.Domain.Entities;

/// <summary>
/// Vaccinul dintr-o schemă de vaccinare, cu intervalul implicit până la rapel.
/// </summary>
public class VaccineType
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public int IntervalMonths { get; init; }
    public bool IsMandatory { get; init; }
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Schema de vaccinare uzuală în cabinetele din România. Intervalele sunt cele
/// de rapel pentru un animal adult, deja intrat în schemă.
/// </summary>
public static class VaccineCatalog
{
    public const string AnySpecies = "Toate";
    public const string Dog = "Câine";
    public const string Cat = "Pisică";

    public static readonly IReadOnlyList<VaccineType> All = new List<VaccineType>
    {
        new()
        {
            Code = "RABIES",
            Name = "Antirabic",
            Species = AnySpecies,
            IntervalMonths = 12,
            IsMandatory = true,
            Description = "Obligatoriu prin lege, se consemnează în carnetul de sănătate."
        },
        new()
        {
            Code = "DHPPI",
            Name = "DHPPi (jigodie, hepatită, parvoviroză, parainfluenza)",
            Species = Dog,
            IntervalMonths = 12,
            IsMandatory = false,
            Description = "Polivalentul de bază pentru câine."
        },
        new()
        {
            Code = "LEPTO",
            Name = "Leptospiroză",
            Species = Dog,
            IntervalMonths = 12,
            IsMandatory = false,
            Description = "Recomandat anual, mai ales pentru câinii cu acces în exterior."
        },
        new()
        {
            Code = "KENNEL_COUGH",
            Name = "Tuse de canisă (Bordetella)",
            Species = Dog,
            IntervalMonths = 12,
            IsMandatory = false,
            Description = "Cerut frecvent de pensiuni și saloane de cosmetică."
        },
        new()
        {
            Code = "FELINE_RCP",
            Name = "Trivalentă felină (rinotraheită, calicivirus, panleucopenie)",
            Species = Cat,
            IntervalMonths = 12,
            IsMandatory = false,
            Description = "Polivalentul de bază pentru pisică."
        },
        new()
        {
            Code = "FELV",
            Name = "Leucemie felină (FeLV)",
            Species = Cat,
            IntervalMonths = 12,
            IsMandatory = false,
            Description = "Recomandat pentru pisicile cu acces în exterior."
        },
        new()
        {
            Code = "DEWORMING",
            Name = "Deparazitare internă",
            Species = AnySpecies,
            IntervalMonths = 3,
            IsMandatory = false,
            Description = "Se repetă trimestrial."
        },
        new()
        {
            Code = "EXTERNAL_PARASITES",
            Name = "Deparazitare externă",
            Species = AnySpecies,
            IntervalMonths = 1,
            IsMandatory = false,
            Description = "Pipetă sau comprimat, lunar în sezonul cald."
        }
    };

    public static VaccineType? Find(string code)
        => All.FirstOrDefault(v => string.Equals(v.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Vaccinurile aplicabile unei specii; cele generale apar pentru orice animal.
    /// </summary>
    public static IEnumerable<VaccineType> ForSpecies(string species)
        => All.Where(v => v.Species == AnySpecies ||
                          string.Equals(v.Species, species, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Data rapelului calculată din catalog. Dacă nu găsim codul, cădem pe un an.
    /// </summary>
    public static DateTime NextDueDate(string code, DateTime administeredOn)
    {
        var months = Find(code)?.IntervalMonths ?? 12;
        return administeredOn.Date.AddMonths(months);
    }
}

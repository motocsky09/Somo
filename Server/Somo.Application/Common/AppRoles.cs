namespace Somo.Application.Common;

public static class AppRoles
{
    public const string Owner = "Owner";
    public const string Vet = "Vet";
    public const string ClinicAdmin = "ClinicAdmin";
    public const string SomoAdmin = "SomoAdmin";

    public static readonly string[] All = { Owner, Vet, ClinicAdmin, SomoAdmin };
    public static readonly string[] SelfService = { Owner, ClinicAdmin };
}

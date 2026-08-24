using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities;

public class Vaccination
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string PetId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string VetId { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;

    public string VaccineCode { get; set; } = string.Empty;
    public string VaccineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public DateTime AdministeredOn { get; set; }
    public DateTime NextDueOn { get; set; }

    /// <summary>
    /// Se completează când pleacă reminderul de rapel, ca să nu fie trimis de două ori.
    /// </summary>
    public DateTime? ReminderSentAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

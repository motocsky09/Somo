using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities;

public class MedicalRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string PetId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string VetId { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Greutatea măsurată la vizita respectivă, în kilograme.
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// Temperatura măsurată la vizită, în grade Celsius. 0 înseamnă nemăsurată.
    /// </summary>
    public double Temperature { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

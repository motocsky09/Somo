using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities;

public class MedicalRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string PetId { get; set; } = string.Empty;
    public string VetId { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
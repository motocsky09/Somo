using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}

public class Appointment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string PetId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string VetId { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
}
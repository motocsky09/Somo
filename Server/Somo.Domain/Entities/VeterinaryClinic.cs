using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities;

public enum ClinicStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class ClinicPrice
{
    public string Service { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class VeterinaryClinic
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public List<string> VetNames { get; set; } = new();
    public List<ClinicPrice> Prices { get; set; } = new();
    public List<string> VetIds { get; set; } = new();
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [BsonRepresentation(BsonType.String)]
    public string AdminId { get; set; } = string.Empty;

    public ClinicStatus Status { get; set; } = ClinicStatus.Pending;
    public string RejectionReason { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }
}

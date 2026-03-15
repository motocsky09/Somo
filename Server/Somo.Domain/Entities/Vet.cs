using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities;

public class Vet
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public List<string> ClinicIds { get; set; } = new();
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities;

public class VeterinaryClinic
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public List<string> VetIds { get; set; } = new();
    public double Latitude { get; set; } 
    public double Longitude { get; set; }  
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities
{
    public class Medics{

		[BsonId(IdGenerator = typeof(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator))]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		[BsonElement("firstName")]
		public string? FirstName { get; set; }

		[BsonElement("lastName")]
		public string? LastName { get; set; }

		[BsonElement("specialization")]
		public string? Specialization { get; set; }

		[BsonElement("Timetable")]
		public string? Timetable { get; set; }

		[BsonElement("phoneNumber")]
		public string? PhoneNumber { get; set; }

		[BsonElement("email")]
		public string? Email { get; set; }
		
	}
}
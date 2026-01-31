using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Server.Entities
{		
public class MedServices{

		[BsonId(IdGenerator = typeof(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator))]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		[BsonElement("serviceName")]
		public string? ServiceName { get; set; }

		[BsonElement("description")]
		public string? Description { get; set; }

		[BsonElement("price")]
		public decimal? Price { get; set; }

		[BsonElement("durationInMinutes")]
		public int? DurationInMinutes { get; set; }

	}
}

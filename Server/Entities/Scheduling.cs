using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Server.Entities
{		
public class Scheduling{

		[BsonId(IdGenerator = typeof(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator))]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		[BsonElement("ScheduleName")]
		public string? ScheduleName { get; set; }

		[BsonElement("scheduledDateTime")]
		public String? ScheduledDateTime { get; set; }

		[BsonElement("Appointment Time")]
		public String? AppointmentTime { get; set; }

		[BsonElement("Doc Name")]
		public string? DocName { get; set; }

		[BsonElement("customerName")]
		public string? CustomerName { get; set; }

		[BsonElement("PhoneNumber")]
		public string? PhoneNumber { get; set; }

		[BsonElement("Email")]
		public string? Email { get; set; }

	}
}
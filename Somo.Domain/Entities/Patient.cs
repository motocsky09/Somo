using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Domain.Entities
{
	public class Patient
	{
		[BsonId(IdGenerator = typeof(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator))]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		[BsonElement("OwnerName")]
		public string? OwnerName { get; set; }

		[BsonElement("PhoneNumberOwner")]
		public string? PhoneNumberOwner { get; set; }

		[BsonElement("EmailOwner")]
		public string? EmailOwner { get; set; }

		[BsonElement("DocName")]
		public string? DocName { get; set; }

		[BsonElement("Status")]
		public string? Status { get; set; }

		[BsonElement("Notes")]
		public string? Notes { get; set; }

		[BsonElement("Documents")]
		public List<string>? Documents { get; set; }

		[BsonElement("CommunicationHistory")]
		public List<string>? CommunicationHistory { get; set; }

		[BsonElement("FileType")]
		public bool? FileType { get; set; }
		
	}
	
}
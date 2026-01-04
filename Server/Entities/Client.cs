using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Server.Entities
{
    public class Client
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        public string LastName { get; set; }

        [BsonElement("cnp")]
        public string CNP { get; set; }

        [BsonElement("gender")]
        public string Gender { get; set; }

        [BsonElement("citizenship")]
        public string Citizenship { get; set; }

        [BsonElement("age")]
        public int Age { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("postalCode")]
        public string PostalCode { get; set; }

        [BsonElement("county")]
        public string County { get; set; }

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }
    }
}


using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Somo.Server.Entities
{
    public class UserAccount
    {
        [BsonId(IdGenerator = typeof(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string? Username { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}


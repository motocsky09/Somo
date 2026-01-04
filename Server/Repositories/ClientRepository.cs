using MongoDB.Driver;
using Somo.Server.Entities;

namespace Somo.Server.Repositories
{
    public class ClientRepository : MongoRepository<Client>, IClientRepository
    {
        public ClientRepository(IMongoDatabase database) : base(database, "Clients")
        {
        }
    }
}

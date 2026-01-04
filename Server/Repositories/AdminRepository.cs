using MongoDB.Driver;
using Somo.Server.Entities;

namespace Somo.Server.Repositories
{
    public class AdminRepository : MongoRepository<Admin>, IAdminRepository
    {
        public AdminRepository(IMongoDatabase database) : base(database, "Admins")
        {
        }
    }
}

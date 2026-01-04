using MongoDB.Driver;
using Somo.Server.Entities;

namespace Somo.Server.Repositories
{
    public class UserAccountRepository : MongoRepository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(IMongoDatabase database) : base(database, "UserAccounts")
        {
        }
    }
}


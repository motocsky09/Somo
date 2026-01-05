using MongoDB.Driver;
using Somo.Server.Entities;
using System.Threading.Tasks;

namespace Somo.Server.Repositories
{
    public class UserAccountRepository : MongoRepository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(IMongoDatabase database) : base(database, "UserAccounts")
        {
        }

        public async Task<UserAccount> GetByUsernameAsync(string username)
        {
            var filter = Builders<UserAccount>.Filter.Eq(u => u.Username, username);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}


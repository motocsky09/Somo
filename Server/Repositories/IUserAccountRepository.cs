using Somo.Server.Entities;
using System.Threading.Tasks;

namespace Somo.Server.Repositories
{
    public interface IUserAccountRepository : IRepository<UserAccount>
    {
        Task<UserAccount> GetByUsernameAsync(string username);
    }
}



using System.Threading.Tasks;

namespace SG.Core.Features.User
{
    public interface IUserDataService
    {
        Task<User> GetByUsername(string operatorId, string playerId);
    }
    public class UserDataService : IUserDataService
    {
        private readonly IUserDataAccess userDataAccess;

        public UserDataService(IUserDataAccess userDataAccess)
        {
            this.userDataAccess = userDataAccess;
        }

        public Task<User> GetByUsername(string operatorId, string playerId)
         => string.IsNullOrWhiteSpace(operatorId) || string.IsNullOrWhiteSpace(playerId)
            ? Task.FromResult<User>(default)
            : userDataAccess.GetByUsername(operatorId, playerId);
    }
}

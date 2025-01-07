using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;
using SG.Core.Shared.Utils;

namespace SG.Core.Features.User
{
    public interface IUserDataAccess
    {
        Task<User> GetByUsername(string operatorId, string playerId);
    }
    public class UserDataAccess : IUserDataAccess
    {
        private const string collectionName = "users";
        private readonly IMongoCollection<User> userCollection;

        public UserDataAccess(IMongoClient mongoClient, IDatabaseSettings databaseSettings)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.GetMinigameDatabase(databaseSettings.PrefixDbName));
            userCollection = database.GetCollection<User>(collectionName);
        }

        public async Task<User> GetByUsername(string operatorId, string playerId)
         => (await userCollection.FindAsync(x => string.Equals(x.Local.Username, StringFormat.FormatUserName(operatorId, playerId)))).FirstOrDefault();   
    }
}

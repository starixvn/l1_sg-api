using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.Log
{
    public interface ILogDataAccess
    {
        Task WriteRetrieveEndRoundAsync(SBLog log);
        Task WriteSignInAsync(SBLog log);
    }

    public class LogDataAccess : ILogDataAccess
    {
        private const string retrieveEndRoundCollectionName = "retrieve_requests";
        private const string SignInCollectionName = "signin_requests";

        private readonly IMongoCollection<SBLog> retrieveCollection;
        private readonly IMongoCollection<SBLog> signInCollection;

        public LogDataAccess(IMongoClient mongoClient, IDatabaseSettings databaseSettings)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.GetLogDatabase(databaseSettings.PrefixDbName));

            retrieveCollection = database.GetCollection<SBLog>(retrieveEndRoundCollectionName);
            signInCollection = database.GetCollection<SBLog>(SignInCollectionName);
        }

        public Task WriteRetrieveEndRoundAsync(SBLog log)
            => retrieveCollection.InsertOneAsync(log);

        public Task WriteSignInAsync(SBLog log)
            => signInCollection.InsertOneAsync(log);
    }
}
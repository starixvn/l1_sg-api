using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.Log
{
    public interface ILogDataAccess
    {
        Task WriteRetrieveEndRoundAsync(SBLog log);
    }

    public class LogDataAccess : ILogDataAccess
    {
        private const string retrieveEndRoundCollectionName = "retrieve_requests";

        private readonly IMongoCollection<SBLog> retrieveCollection;

        public LogDataAccess(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.Log);

            retrieveCollection = database.GetCollection<SBLog>(retrieveEndRoundCollectionName);
        }

        public Task WriteRetrieveEndRoundAsync(SBLog log)
            => retrieveCollection.InsertOneAsync(log);
    }
}
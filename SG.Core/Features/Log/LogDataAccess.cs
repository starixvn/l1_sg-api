using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.Log
{
    public interface ILogDataAccess
    {
        Task WriteRetrieveEndRoundAsync(SBLog log);
        Task WriteSignInAsync(SBLog log);
        Task WritePromotionAsync(SBLog log);
    }

    public class LogDataAccess : ILogDataAccess
    {
        private const string retrieveEndRoundCollectionName = "retrieve_requests";
        private const string SignInCollectionName = "signin_requests";
        private const string PromotionCollectionName = "promotion_requests";

        private readonly IMongoCollection<SBLog> retrieveCollection;
        private readonly IMongoCollection<SBLog> signInCollection;
        private readonly IMongoCollection<SBLog> promotionCollection;

        public LogDataAccess(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.Log);

            retrieveCollection = database.GetCollection<SBLog>(retrieveEndRoundCollectionName);
            signInCollection = database.GetCollection<SBLog>(SignInCollectionName);
            promotionCollection = database.GetCollection<SBLog>(PromotionCollectionName);
        }

        public Task WriteRetrieveEndRoundAsync(SBLog log)
            => retrieveCollection.InsertOneAsync(log);

        public Task WriteSignInAsync(SBLog log)
            => signInCollection.InsertOneAsync(log);
            
        public Task WritePromotionAsync(SBLog log)
            => promotionCollection.InsertOneAsync(log);
    }
}
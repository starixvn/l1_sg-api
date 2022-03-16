using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.Operators
{
    public interface IOperatorDataAccess
    {
        Task<Operator> GetById(string operatorId);

        Task<bool> Verify(string operatorId, string secretKey);
    }

    public class OperatorDataAccess : IOperatorDataAccess
    {
        private const string collectionName = "partners";
        private readonly IMongoCollection<Operator> partnerCollection;

        public OperatorDataAccess(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.Minigame);
            partnerCollection = database.GetCollection<Operator>(collectionName);
        }

        public async Task<Operator> GetById(string operatorId) =>
            (await partnerCollection.FindAsync(x => x.PartnerId == operatorId)).FirstOrDefault();

        public async Task<bool> Verify(string operatorId, string secretKey)
        => (await partnerCollection.CountDocumentsAsync(x => x.PartnerId == operatorId && x.SecretKey == secretKey)) > 0;
    }
}
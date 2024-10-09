using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.Operators
{
    public interface ICurrencyGroupDataAccess
    {
        Task<CurrencyGroup> GetById(string currencyGroupId);
    }

    public class CurrencyGroupDataAccess : ICurrencyGroupDataAccess
    {
        private const string collectionName = "currency_groups";
        private readonly IMongoCollection<CurrencyGroup> currencyGroupCollection;

        public CurrencyGroupDataAccess(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.Minigame);
            currencyGroupCollection = database.GetCollection<CurrencyGroup>(collectionName);
        }

        public async Task<CurrencyGroup> GetById(string currencyGroupId) =>
            (await currencyGroupCollection.FindAsync(x => x.Id == currencyGroupId)).FirstOrDefault();
    }
}
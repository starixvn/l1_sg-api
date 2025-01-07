using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.Operators
{
    public interface ICurrencyDataAccess
    {
        Task<bool> Verify(string partnerId, string currencyName);

        Task<Currency> Get(string partnerId, string currencyName);
    }

    public class CurrencyDataAccess : ICurrencyDataAccess
    {
        private const string collectionName = "currencies";
        private readonly Collation defaultCollation = new("en", strength: CollationStrength.Secondary);

        private readonly IMongoCollection<Currency> currencyCollection;

        public CurrencyDataAccess(IMongoClient mongoClient, IDatabaseSettings databaseSettings)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.GetMinigameDatabase(databaseSettings.PrefixDbName));
            currencyCollection = database.GetCollection<Currency>(collectionName);
        }

        public async Task<bool> Verify(string partnerId, string currencyName)
        => (await currencyCollection.CountDocumentsAsync(
                currency => currency.PartnerId == partnerId &&
                            currency.Name.ToUpperInvariant() == currencyName.ToUpperInvariant() &&
                            currency.Enabled,
                new CountOptions
                {
                    Collation = defaultCollation
                })) > 0;

        public async Task<Currency> Get(string partnerId, string currencyName)
        => (await currencyCollection.FindAsync(
                currency => currency.PartnerId == partnerId &&
                            currency.Name.ToUpperInvariant() == currencyName.ToUpperInvariant() &&
                            currency.Enabled,
                new FindOptions<Currency, Currency>
                {
                    Collation = defaultCollation
                })).FirstOrDefault();
    }
}
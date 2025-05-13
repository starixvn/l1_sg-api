using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.Promotions
{
    public interface IPromotionDataAccess
    {
        Task<Promotion> GetByCodeAsync(string promotionCode, string operatorId);
    }

    public class PromotionDataAccess : IPromotionDataAccess
    {
        private const string collectionName = "game_promotions";
        private readonly IMongoCollection<Promotion> promotionCollection;

        public PromotionDataAccess(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.Minigame);
            promotionCollection = database.GetCollection<Promotion>(collectionName);
        }

        public async Task<Promotion> GetByCodeAsync(string promotionCode, string operatorId)
        {
            return (await promotionCollection.FindAsync(
                promotion => promotion.PromotionCode == promotionCode && 
                             promotion.OperatorId == operatorId))
                .FirstOrDefault();
        }
    }
} 
using System;
using System.Threading.Tasks;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.Promotions
{
    public interface IPromotionService
    {
        Task<PromotionResult> GetPromotionAsync(string promotionCode, string operatorId, string seq);
    }

    public class PromotionService : IPromotionService
    {
        private readonly IPromotionDataAccess promotionDataAccess;

        public PromotionService(IPromotionDataAccess promotionDataAccess)
        {
            this.promotionDataAccess = promotionDataAccess;
        }

        public async Task<PromotionResult> GetPromotionAsync(string promotionCode, string operatorId, string seq)
        {
            var promotion = await promotionDataAccess.GetByCodeAsync(promotionCode, operatorId);

            if (promotion == null)
            {
                return new PromotionResult
                {
                    Seq = seq,
                    ErrorCode = ErrorCodes.InternalError,
                    ErrorDescription = "Promotion not found"
                };
            }

            return new PromotionResult
            {
                Seq = seq,
                PromotionCode = promotion.PromotionCode,
                PromotionName = promotion.PromotionName,
                TotalAmount = 200, // Mock value
                TotalBetCount = 500, // Mock value
                Status = promotion.GetStatus(),
                EndTime = promotion.EndDate,
                ErrorCode = ErrorCodes.SuccessCode,
                ErrorDescription = "Success"
            };
        }
    }
} 
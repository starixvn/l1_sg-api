using System;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using SG.Core.Features.Promotions;
using SG.Core.Shared.Constants;
using Xunit;

namespace SG.Core.Tests.Features.Promotions
{
    public class PromotionServiceTests
    {
        private readonly IPromotionDataAccess promotionDataAccess;
        private readonly PromotionService promotionService;
        private readonly Fixture fixture = new();

        public PromotionServiceTests()
        {
            promotionDataAccess = Substitute.For<IPromotionDataAccess>();
            promotionService = new PromotionService(promotionDataAccess);
        }

        [Fact]
        public async Task GetPromotionAsync_PromotionExists_ReturnsPromotionResult()
        {
            // Arrange
            var promotion = fixture.Create<Promotion>();
            var promotionCode = promotion.PromotionCode;
            var operatorId = promotion.OperatorId;
            var seq = fixture.Create<string>();

            promotionDataAccess.GetByCodeAsync(promotionCode, operatorId).Returns(promotion);

            // Act
            var result = await promotionService.GetPromotionAsync(promotionCode, operatorId, seq);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(seq, result.Seq);
            Assert.Equal(promotion.PromotionCode, result.PromotionCode);
            Assert.Equal(promotion.PromotionName, result.PromotionName);
            Assert.Equal(promotion.EndDate, result.EndTime);
            Assert.Equal(ErrorCodes.SuccessCode, result.ErrorCode);
            Assert.Equal("Success", result.ErrorDescription);
        }

        [Fact]
        public async Task GetPromotionAsync_PromotionNotFound_ReturnsErrorResult()
        {
            // Arrange
            var promotionCode = fixture.Create<string>();
            var operatorId = fixture.Create<string>();
            var seq = fixture.Create<string>();

            promotionDataAccess.GetByCodeAsync(promotionCode, operatorId).Returns((Promotion)null);

            // Act
            var result = await promotionService.GetPromotionAsync(promotionCode, operatorId, seq);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(seq, result.Seq);
            Assert.Equal(ErrorCodes.InternalError, result.ErrorCode);
            Assert.Equal("Promotion not found", result.ErrorDescription);
        }
    }
} 
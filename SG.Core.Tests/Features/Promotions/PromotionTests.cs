using System;
using SG.Core.Features.Promotions;
using Xunit;

namespace SG.Core.Tests.Features.Promotions
{
    public class PromotionTests
    {
        [Fact]
        public void GetStatus_CurrentDateBeforeStartDate_ReturnsUpcoming()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var promotion = new Promotion
            {
                StartDate = now.AddDays(1),
                EndDate = now.AddDays(10)
            };

            // Act
            var status = promotion.GetStatus();

            // Assert
            Assert.Equal(PromotionStatus.Upcoming, status);
        }

        [Fact]
        public void GetStatus_CurrentDateAfterEndDate_ReturnsCompleted()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var promotion = new Promotion
            {
                StartDate = now.AddDays(-10),
                EndDate = now.AddDays(-1)
            };

            // Act
            var status = promotion.GetStatus();

            // Assert
            Assert.Equal(PromotionStatus.Completed, status);
        }

        [Fact]
        public void GetStatus_CurrentDateBetweenStartAndEndDates_ReturnsRunning()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var promotion = new Promotion
            {
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(10)
            };

            // Act
            var status = promotion.GetStatus();

            // Assert
            Assert.Equal(PromotionStatus.Running, status);
        }

        [Fact]
        public void GetStatus_CurrentDateEqualsStartDate_ReturnsRunning()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var promotion = new Promotion
            {
                StartDate = now,
                EndDate = now.AddDays(10)
            };

            // Act
            var status = promotion.GetStatus();

            // Assert
            Assert.Equal(PromotionStatus.Running, status);
        }

        [Fact]
        public void GetStatus_CurrentDateEqualsEndDate_ReturnsCompleted()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var promotion = new Promotion
            {
                StartDate = now.AddDays(-1),
                EndDate = now
            };

            // Act
            var status = promotion.GetStatus();

            // Assert
            Assert.Equal(PromotionStatus.Completed, status);
        }
    }
} 
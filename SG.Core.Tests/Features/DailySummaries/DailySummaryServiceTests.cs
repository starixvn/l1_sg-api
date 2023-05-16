using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using SG.Core.Features.DailySummaries;
using SG.Core.Shared.Configurations;
using Xunit;

namespace SG.Core.Tests.Features.DailySummaries
{
    public class DailySummaryServiceTests
    {
        private readonly IDailySummaryService dailySummaryService;
        private readonly IDailySummaryDataAccess dataAccess;
        private readonly IAppSettings appSettings;
        private readonly Fixture fixture = new();

        public DailySummaryServiceTests()
        {
            dataAccess = Substitute.For<IDailySummaryDataAccess>();
            appSettings = Substitute.For<IAppSettings>();

            dailySummaryService = new DailySummaryService(dataAccess, appSettings);
        }

        [Theory, AutoData]
        public async Task Get_ValidInputData_ReturnCorrectData(DateTimeOffset date, string operatorId)
        {
            // Arrange
            dataAccess.Get(date, operatorId).Returns(fixture.Create<IEnumerable<DailySummary>>());
            appSettings.DailySummaryConsolidationMapping.Returns(new Dictionary<string, string>());

            // Act
            var result = await dailySummaryService.Get(date, operatorId);

            // Assert
            await dataAccess.Received().Get(date, operatorId);
            Assert.NotEmpty(result.Summary);
        }

        [Fact]
        public async Task Get_HasSubOperator_ReturnConsolidateDailySummary()
        {
            // Arrange
            var summaryDate = new DateTime(1989, 05, 28);
            var mainOperatorId = "mainOperatorId";
            var subOperatorId = "subOperatorId";
            var operatorMapping = new Dictionary<string, string>
            {
                { mainOperatorId, subOperatorId }
            };
            ArrangeOperatorSummaryData(summaryDate, mainOperatorId, subOperatorId, operatorMapping);

            //// Act
            var result = await dailySummaryService.Get(summaryDate, mainOperatorId);

            //// Assert
            Assert.Equal(2, result.Summary.Count());

            var actualUSSumary = result.Summary.FirstOrDefault(sumItem => sumItem.Currency == "US");
            Assert.Equal(130, actualUSSumary.Stake);
            Assert.Equal(340, actualUSSumary.Payout);
            Assert.Equal(300, actualUSSumary.TicketCount);

            var actualINSumary = result.Summary.FirstOrDefault(sumItem => sumItem.Currency == "IN");
            Assert.Equal(25, actualINSumary.Stake);
            Assert.Equal(10, actualINSumary.Payout);
            Assert.Equal(16, actualINSumary.TicketCount);
        }

        [Fact]
        public async Task Get_NoMappingSubOperator_ReturnOnlyMainOperatorDailySummary()
        {
            // Arrange
            var summaryDate = new DateTime(1989, 05, 28);
            var mainOperatorId = "mainOperatorId";
            var subOperatorId = "subOperatorId";
            var operatorMapping = new Dictionary<string, string>
            {
                { "otherMainOperator", subOperatorId }
            };
            ArrangeOperatorSummaryData(summaryDate, mainOperatorId, subOperatorId, operatorMapping);

            //// Act
            var result = await dailySummaryService.Get(summaryDate, mainOperatorId);

            //// Assert
            Assert.Equal(2, result.Summary.Count());

            var actualUSSumary = result.Summary.FirstOrDefault(sumItem => sumItem.Currency == "US");
            Assert.Equal(15, actualUSSumary.Stake);
            Assert.Equal(20, actualUSSumary.Payout);
            Assert.Equal(100, actualUSSumary.TicketCount);

            var actualINSumary = result.Summary.FirstOrDefault(sumItem => sumItem.Currency == "IN");
            Assert.Equal(25, actualINSumary.Stake);
            Assert.Equal(10, actualINSumary.Payout);
            Assert.Equal(16, actualINSumary.TicketCount);
        }

        private void ArrangeOperatorSummaryData(DateTime summaryDate, string mainOperatorId, string subOperatorId, Dictionary<string, string> operatorMapping)
        {
            dataAccess.Get(summaryDate, mainOperatorId).Returns(Task.FromResult(new List<DailySummary> {
                    new DailySummary
                    {
                        Currency = "US",
                        Stake = 15,
                        Payout = 20,
                        Tickets = 100
                    },
                    new DailySummary
                    {
                        Currency = "IN",
                        Stake = 25,
                        Payout = 10,
                        Tickets = 16
                    }
                } as IEnumerable<DailySummary>));
            dataAccess.Get(summaryDate, subOperatorId).Returns(Task.FromResult(new List<DailySummary> {
                    new DailySummary
                    {
                        Currency = "US",
                        Stake = 115,
                        Payout = 320,
                        Tickets = 200
                    }
                } as IEnumerable<DailySummary>));
            appSettings.DailySummaryConsolidationMapping.Returns(operatorMapping);
        }
    }
}
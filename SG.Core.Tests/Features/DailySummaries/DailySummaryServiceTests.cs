using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using SG.Core.Features.DailySummaries;
using Xunit;

namespace SG.Core.Tests.Features.DailySummaries
{
    public class DailySummaryServiceTests
    {
        private readonly IDailySummaryService dailySummaryService;
        private readonly IDailySummaryDataAccess dataAccess;
        private readonly Fixture fixture = new();

        public DailySummaryServiceTests()
        {
            dataAccess = Substitute.For<IDailySummaryDataAccess>();

            dailySummaryService = new DailySummaryService(dataAccess);
        }

        [Theory, AutoData]
        public async Task Get_ValidInputData_ReturnCorrectData(DateTimeOffset date, string operatorId)
        {
            // Arrange
            dataAccess.Get(date, operatorId).Returns(fixture.Create<IEnumerable<DailySummary>>());

            // Act
            var result = await dailySummaryService.Get(date, operatorId);

            // Assert
            await dataAccess.Received().Get(date, operatorId);
            Assert.NotEmpty(result.Summary);
        }
    }
}
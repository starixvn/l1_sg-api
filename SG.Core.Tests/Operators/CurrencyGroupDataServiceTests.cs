using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using SG.Core.Features.Operators;
using Xunit;

namespace SG.Core.Tests.Operators
{
    public class CurrencyGroupDataServiceTests
    {
        private readonly ICurrencyGroupDataAccess operatorDataAccess;
        private readonly Fixture fixture = new();

        private readonly CurrencyGroupDataService currencyGroupDataService;

        public CurrencyGroupDataServiceTests()
        {
            operatorDataAccess = Substitute.For<ICurrencyGroupDataAccess>();

            currencyGroupDataService = new CurrencyGroupDataService(operatorDataAccess);
        }

        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        [Theory]
        public async Task GetById_IdIsNullOrWhiteSpaces_ReturnNull(string id)
        {
            // Arrange

            // Act
            var result = await currencyGroupDataService.GetById(id);

            // Assert
            Assert.Null(result);
            await operatorDataAccess.DidNotReceive().GetById(id);
        }

        [Theory, AutoData]
        public async Task GetById_IdNotNull_ReturnCorrectValue(string id)
        {
            // Arrange
            var currrencyGroup = fixture.Create<CurrencyGroup>();
            operatorDataAccess.GetById(id).Returns(currrencyGroup);

            // Act
            var result = await currencyGroupDataService.GetById(id);

            // Assert
            Assert.Equal(currrencyGroup, result);
            await operatorDataAccess.Received(1).GetById(id);
        }
    }
}
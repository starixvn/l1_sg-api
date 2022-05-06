using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using SG.Core.Features.Operators;
using Xunit;

namespace SG.Core.Tests.Operators
{
    public class OperatorDataServiceTests
    {
        private readonly IOperatorDataAccess operatorDataAccess;
        private readonly ICurrencyDataAccess currencyDataAccess;
        private readonly Fixture fixture = new();

        private readonly OperatorDataService operatorDataService;

        public OperatorDataServiceTests()
        {
            operatorDataAccess = Substitute.For<IOperatorDataAccess>();
            currencyDataAccess = Substitute.For<ICurrencyDataAccess>();

            operatorDataService = new OperatorDataService(operatorDataAccess, currencyDataAccess);
        }

        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        [Theory]
        public async Task GetById_IdIsNullOrWhiteSpaces_ReturnNull(string id)
        {
            // Arrange

            // Act
            var result = await operatorDataService.GetById(id);

            // Assert
            Assert.Null(result);
            await operatorDataAccess.DidNotReceive().GetById(id);
        }

        [Theory, AutoData]
        public async Task GetById_IdNotNull_ReturnCorrectValue(string id)
        {
            // Arrange
            var @operator = fixture.Create<Operator>();
            operatorDataAccess.GetById(id).Returns(@operator);

            // Act
            var result = await operatorDataService.GetById(id);

            // Assert
            Assert.Equal(@operator, result);
            await operatorDataAccess.Received(1).GetById(id);
        }

        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("   ", "   ")]
        [Theory]
        public async Task GetCurrency_OperatorIdIsNullOrWhiteSpaces_ReturnNull(string operatorId, string currencyName)
        {
            // Arrange

            // Act
            var result = await operatorDataService.GetCurrency(operatorId, currencyName);

            // Assert
            Assert.Null(result);
            await currencyDataAccess.DidNotReceive().Get(operatorId, currencyName);
        }

        [Theory, AutoData]
        public async Task GetCurrency_OperatorIdNotNull_ReturnCorrectValue(string operatorId, string currencyName)
        {
            // Arrange
            var currency = fixture.Create<Currency>();
            currencyDataAccess.Get(operatorId, currencyName).Returns(currency);

            // Act
            var result = await operatorDataService.GetCurrency(operatorId, currencyName);

            // Assert
            Assert.Equal(currency, result);
            await currencyDataAccess.Received(1).Get(operatorId, currencyName);
        }

        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("   ", "   ")]
        [Theory]
        public async Task Verify_OperatorIdIsNullOrWhiteSpaces_ReturnFalse(string operatorId, string secretKey)
        {
            // Arrange

            // Act
            var result = await operatorDataService.VerifyWithCurrency(operatorId, secretKey);

            // Assert
            Assert.False(result);
            await operatorDataAccess.DidNotReceive().Verify(operatorId, secretKey);
        }

        [Theory, AutoData]
        public async Task Verify_OperatorIdNotNull_ReturnCorrectValue(string operatorId, string secretKey)
        {
            // Arrange
            var isVerify = fixture.Create<bool>();
            currencyDataAccess.Verify(operatorId, secretKey).Returns(isVerify);

            // Act
            var result = await operatorDataService.VerifyWithCurrency(operatorId, secretKey);

            // Assert
            Assert.Equal(isVerify, result);
            await currencyDataAccess.Received(1).Verify(operatorId, secretKey);
        }

        [Theory, AutoData]
        public async Task VerifyWithCurrency_OperatorIdNotNull_ReturnCorrectValue(string operatorId, string currency)
        {
            // Arrange
            var isVerifiedCurrency = fixture.Create<bool>();
            currencyDataAccess.Verify(operatorId, currency).Returns(isVerifiedCurrency);

            // Act
            var result = await operatorDataService.VerifyWithCurrency(operatorId, currency);

            // Assert
            Assert.Equal(isVerifiedCurrency, result);
            await currencyDataAccess.Received(1).Verify(operatorId, currency);
        }
    }
}
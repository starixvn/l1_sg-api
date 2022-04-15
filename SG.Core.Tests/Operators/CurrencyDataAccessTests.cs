using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using MongoDB.Driver;
using NSubstitute;
using SG.Core.Features.Operators;
using Xunit;

namespace SG.Core.Tests.Operators
{
    public class CurrencyDataAccessTests
    {
        private readonly IMongoCollection<Currency> currencyCollection;

        private readonly CurrencyDataAccess dataAccess;

        public CurrencyDataAccessTests()
        {
            currencyCollection = Substitute.For<IMongoCollection<Currency>>();

            var mongoClient = Substitute.For<IMongoClient>();
            var database = Substitute.For<IMongoDatabase>();
            mongoClient.GetDatabase(Arg.Any<string>()).Returns(database);
            database.GetCollection<Currency>("currencies").Returns(currencyCollection);

            dataAccess = new CurrencyDataAccess(mongoClient);
        }

        [Theory, AutoData]
        public async Task Verify_InvalidCurrency_ReturnApproriateValue(string partnerId, string currencyName)
        {
            // Arrange
            currencyCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Currency>>()).Returns(0);

            // Act
            var actualValue = await dataAccess.Verify(partnerId, currencyName);

            // Assert
            Assert.False(actualValue);
        }

        [Theory, AutoData]
        public async Task Verify_ValidCurrency_ReturnApproriateValue(string partnerId, string currencyName)
        {
            // Arrange
            currencyCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Currency>>(), Arg.Any<CountOptions>()).Returns(1);

            // Act
            var actualValue = await dataAccess.Verify(partnerId, currencyName);

            // Assert
            Assert.True(actualValue);
        }
    }
}

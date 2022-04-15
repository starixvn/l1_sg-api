using System.Threading.Tasks;
using AutoFixture.Xunit2;
using MongoDB.Driver;
using NSubstitute;
using SG.Core.Features.Operators;
using Xunit;

namespace SG.Core.Tests.Operators
{
    public class OperatorDataAccessTests
    {
        private readonly IMongoCollection<Operator> operatorCollection;
        private readonly OperatorDataAccess dataAccess;

        public OperatorDataAccessTests()
        {
            operatorCollection = Substitute.For<IMongoCollection<Operator>>();

            var mongoClient = Substitute.For<IMongoClient>();
            var database = Substitute.For<IMongoDatabase>();
            mongoClient.GetDatabase(Arg.Any<string>()).Returns(database);
            database.GetCollection<Operator>("partners").Returns(operatorCollection);

            dataAccess = new OperatorDataAccess(mongoClient);
        }

        [Theory, AutoData]
        public async Task Verify_InvalidOperator_ReturnApproriateValue(string operatorId, string secretKey)
        {
            // Arrange
            operatorCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Operator>>()).Returns(0);

            // Act
            var actualValue = await dataAccess.Verify(operatorId, secretKey);

            // Assert
            Assert.False(actualValue);
        }

        [Theory, AutoData]
        public async Task Verify_ValidOperator_ReturnApproriateValue(string operatorId, string secretKey)
        {
            // Arrange
            operatorCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Operator>>()).Returns(1);

            // Act
            var actualValue = await dataAccess.Verify(operatorId, secretKey);

            // Assert
            Assert.True(actualValue);
        }
    }
}
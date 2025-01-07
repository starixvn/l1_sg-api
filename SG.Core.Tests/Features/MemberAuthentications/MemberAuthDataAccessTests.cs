using System.Threading.Tasks;
using AutoFixture.Xunit2;
using MongoDB.Driver;
using NSubstitute;
using SG.Core.Features.MemberAuthentications;
using SG.Core.Shared.Configurations;
using Xunit;

namespace SG.Core.Tests.Features.MemberAuthentications
{
    public class MemberAuthDataAccessTests
    {
        private readonly IMongoCollection<MemberAuth> memberAuthCollection;
        private readonly MemberAuthDataAccess dataAccess;

        public MemberAuthDataAccessTests()
        {
            memberAuthCollection = Substitute.For<IMongoCollection<MemberAuth>>();
            var mongoClient = Substitute.For<IMongoClient>();
            var database = Substitute.For<IMongoDatabase>();
            var databaseSettings = Substitute.For<IDatabaseSettings>();
            mongoClient.GetDatabase("member").Returns(database);
            database.GetCollection<MemberAuth>("member_auths").Returns(memberAuthCollection);

            dataAccess = new MemberAuthDataAccess(mongoClient, databaseSettings);
        }

        [Theory, AutoData]
        public async Task Get_Always_ReturnCorrectMemberAuth(string id)
        {
            // Act
            var actualValue = await dataAccess.Get(id);

            // Assert
            Assert.Null(actualValue);
        }

        [Theory, AutoData]
        public async Task GetByToken_Always_ReturnCorrectMemberAuth(string token)
        {
            // Act
            var actualValue = await dataAccess.GetByToken(token);

            // Assert
            Assert.Null(actualValue);
        }

        [Theory, AutoData]
        public async Task GetLatest_Always_ReturnCorrectMemberAuth(MemberAuth memberAuth)
        {
            // Act
            var actualValue = await dataAccess.GetLatest(memberAuth);

            // Assert
            Assert.Null(actualValue);
        }

        [Theory, AutoData]
        public async Task Create_Always_InjectInsertOneAsync(MemberAuth memberAuth)
        {
            // Act
            await dataAccess.Create(memberAuth);

            // Assert
            await memberAuthCollection.Received().InsertOneAsync(memberAuth);
        }

        [Theory, AutoData]
        public async Task UpdateCurrencyGroup_Always_InjectUpdateOneAsync(string token, string currencyGroupId)
        {
            // Act
            await dataAccess.UpdateCurrencyGroup(token, currencyGroupId);

            // Assert
            await memberAuthCollection.Received().UpdateOneAsync(Arg.Any<FilterDefinition<MemberAuth>>(), Arg.Any<UpdateDefinition<MemberAuth>>());
        }
    }
}
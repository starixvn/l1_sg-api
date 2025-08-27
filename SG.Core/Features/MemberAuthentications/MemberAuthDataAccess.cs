using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.MemberAuthentications
{
    public interface IMemberAuthDataAccess
    {
        Task<MemberAuth> Get(string id);

        Task<MemberAuth> GetByToken(string token);

        Task<MemberAuth> Create(MemberAuth memberAuth);

        Task<MemberAuth> GetLatest(MemberAuth memberAuth);

        Task UpdateCurrencyGroup(string token, string currencyGroupId);

        Task<MemberAuth> GetForTestCurrency(string token);
    }

    public class MemberAuthDataAccess : IMemberAuthDataAccess
    {
        private const string collectionName = "member_auths";
        private readonly IMongoCollection<MemberAuth> memberAuthCollection;

        public MemberAuthDataAccess(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.Member);
            memberAuthCollection = database.GetCollection<MemberAuth>(collectionName);
        }

        public async Task<MemberAuth> Get(string id) =>
            (await memberAuthCollection.FindAsync(memberAuth => memberAuth.Id == id)).FirstOrDefault();

        public async Task<MemberAuth> GetLatest(MemberAuth memberAuth)
        {
            var builder = Builders<MemberAuth>.Filter;
            var filter = builder.Eq(x => x.IP, memberAuth.IP) &
                builder.Eq(x => x.PlayerId, memberAuth.PlayerId) &
                builder.Eq(x => x.AppType, memberAuth.AppType) &
                builder.Eq(x => x.SiteId, memberAuth.SiteId) &
                builder.Eq(x => x.BrowserUserAgent, memberAuth.BrowserUserAgent) &
                builder.Eq(x => x.Language, memberAuth.Language) &
                builder.Eq(x => x.CurrencyName, memberAuth.CurrencyName) &
                builder.Eq(x => x.GameId, memberAuth.GameId);

            var options = new FindOptions<MemberAuth, MemberAuth>
            {
                Limit = 1,
                Sort = Builders<MemberAuth>.Sort.Descending(x => x.Time),
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            };

            return (await memberAuthCollection.FindAsync(filter, options)).FirstOrDefault();
        }

        public async Task<MemberAuth> GetByToken(string token) =>
            (await memberAuthCollection.FindAsync(memberAuth => memberAuth.Token == token)).FirstOrDefault();

        public async Task<MemberAuth> Create(MemberAuth memberAuth)
        {
            await memberAuthCollection.InsertOneAsync(memberAuth);

            return memberAuth;
        }

        public Task UpdateCurrencyGroup(string token, string currencyGroupId)
        {
            var updates = Builders<MemberAuth>.Update
                .Set(x => x.CurrencyGroupId, currencyGroupId)
                .Set(x => x.Time, DateTimeOffset.Now);

            return memberAuthCollection.UpdateOneAsync(x => x.Token == token, updates);
        }

        public async Task<MemberAuth> GetForTestCurrency(string token)
        => (await memberAuthCollection.FindAsync(memberAuth => memberAuth.Token == token && memberAuth.IsTestCurrency)).FirstOrDefault();
    }
}
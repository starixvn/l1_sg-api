using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.DailySummaries
{
    public interface IDailySummaryDataAccess
    {
        Task<IEnumerable<DailySummary>> Get(DateTimeOffset summarizeDate, string operatorId, string siteId);
    }

    public class DailySummaryDataAccess : IDailySummaryDataAccess
    {
        private const string collectionName = "daily_summaries";

        private readonly IMongoCollection<DailySummary> collection;

        public DailySummaryDataAccess(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(MongoDatabases.Summary);
            collection = database.GetCollection<DailySummary>(collectionName);
        }

        public async Task<IEnumerable<DailySummary>> Get(DateTimeOffset summarizeDate, string operatorId, string siteId)
            => await collection
            .Find(x => x.Time == summarizeDate.Date && x.OperatorId == operatorId && (string.IsNullOrWhiteSpace(siteId) || x.SiteId == siteId))
            .ToListAsync();
    }
}
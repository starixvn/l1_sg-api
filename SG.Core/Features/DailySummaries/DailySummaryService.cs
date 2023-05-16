using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry;
using SG.Core.Shared.Configurations;

namespace SG.Core.Features.DailySummaries

{
    public interface IDailySummaryService
    {
        Task<DailySummaryResult> Get(DateTimeOffset summarizeDate, string operatorId);
    }

    public class DailySummaryService : IDailySummaryService
    {
        private const int RoundDecimals = 2;

        private readonly IDailySummaryDataAccess dataAccess;
        private readonly IAppSettings appSettings;
        private readonly ISentryClient sentryClient;

        public DailySummaryService(IDailySummaryDataAccess dataAccess, IAppSettings appSettings, ISentryClient sentryClient)
        {
            this.dataAccess = dataAccess;
            this.appSettings = appSettings;
            this.sentryClient = sentryClient;
        }

        public async Task<DailySummaryResult> Get(DateTimeOffset summarizeDate, string operatorId)
        {
            var mainDailySummary = await GetDailySummary(summarizeDate, operatorId);

            if (appSettings.DailySummaryConsolidationMapping.Any() 
                && appSettings.DailySummaryConsolidationMapping.Keys.Contains(operatorId))
            {
                return await ConsolidateSubOperatorsDailySummary(mainDailySummary, operatorId, summarizeDate);
            }

            return mainDailySummary;
        }

        private async Task<DailySummaryResult> GetDailySummary(DateTimeOffset summarizeDate, string operatorId)
        {
            var dailySummaryItems = await dataAccess.Get(summarizeDate, operatorId);

            return new DailySummaryResult
            {
                Summary = dailySummaryItems
                    .GroupBy(x => x.Currency)
                    .Select(g => new SummaryItem(
                        g.Key,
                        decimal.Round(g.Sum(i => i.Stake), RoundDecimals),
                        decimal.Round(g.Sum(i => i.Payout), RoundDecimals),
                        g.Sum(i => i.Tickets)))
            };
        }

        private async Task<DailySummaryResult> ConsolidateSubOperatorsDailySummary(
            DailySummaryResult mainOperatorSummary, 
            string mainOperator,
            DateTimeOffset summarizeDate)
        {
            var allOperatorsDailySymmary = new List<SummaryItem>(mainOperatorSummary.Summary);

            foreach (KeyValuePair<string, string> operatorMapping in appSettings.DailySummaryConsolidationMapping)
            {
                if (mainOperator == operatorMapping.Key)
                {
                    var subDailySummary = await GetDailySummary(summarizeDate, operatorMapping.Value);

                    allOperatorsDailySymmary.AddRange(subDailySummary.Summary);
                }
            }

            sentryClient.CaptureMessage(JsonConvert.SerializeObject(allOperatorsDailySymmary));

            return new DailySummaryResult
            {
                Summary = SumByCurrency(allOperatorsDailySymmary)
            };
        }

        private static List<SummaryItem> SumByCurrency(List<SummaryItem> totalSummary)
        {
            var groupedTotalSummary = totalSummary
                .GroupBy(summaryItem => summaryItem.Currency)
                .Select(g => new SummaryItem(
                    g.Key,
                    g.Select(summaryItem => summaryItem.Stake).Sum(),
                    g.Select(summaryItem => summaryItem.Payout).Sum(),
                    g.Select(summaryItem => summaryItem.TicketCount).Sum()
                ));

            return groupedTotalSummary.ToList();
        }
    }
}
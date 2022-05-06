using System;
using System.Linq;
using System.Threading.Tasks;

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

        public DailySummaryService(IDailySummaryDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task<DailySummaryResult> Get(DateTimeOffset summarizeDate, string operatorId)
        {
            var result = await dataAccess.Get(summarizeDate, operatorId);

            return new DailySummaryResult
            {
                Summary = result
                    .GroupBy(x => x.Currency)
                    .Select(g => new SummaryItem(
                        g.Key,
                        decimal.Round(g.Sum(i => i.Stake), RoundDecimals),
                        decimal.Round(g.Sum(i => i.Payout), RoundDecimals),
                        g.Sum(i => i.Tickets)))
            };
        }
    }
}
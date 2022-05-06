using System.Collections.Generic;
using SG.Core.Shared.Models;

namespace SG.Core.Features.DailySummaries
{
    public class DailySummaryResult : SGResult
    {
        public IEnumerable<SummaryItem> Summary { get; set; }
    }

    public class SummaryItem
    {
        public SummaryItem(
          string currency,
          decimal stake,
          decimal payout,
          int ticketCount)
        {
            Currency = currency;
            Stake = stake;
            Payout = payout;
            TicketCount = ticketCount;
        }

        public string Currency { get; set; }

        public decimal Stake { get; set; }

        public decimal Payout { get; set; }

        public int TicketCount { get; set; }
    }
}
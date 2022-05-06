using System;
using SG.Core.Shared.Models;

namespace SG.Core.Features.EndRoundInfo
{
    public class EndRoundInfoResult : SGResult
    {
        public decimal TotalAmount { get; set; }

        public decimal TotalWin { get; set; }

        public decimal ValidBetAmount { get; set; }

        public DateTimeOffset? EndTime { get; set; }
    }
}
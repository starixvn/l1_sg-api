using System;
using System.Collections.Generic;
using SG.Core.Shared.Models;

namespace SG.Core.Features.EndRoundInfo
{
    public class EndRoundInfoResult : SGResult
    {
        public decimal TotalAmount { get; set; }

        public decimal TotalWin { get; set; }

        public decimal ValidBetAmount { get; set; }

        public DateTimeOffset? EndTime { get; set; }

        public MetaData MetaData { get; set; }
    }

    public class MetaData
    {
        public List<BetDetail> BetDetails { get; set; }

        public string Result { get; set; }

        public string GuaranteedWinStatus { get; set; }

        public bool GuaranteedWinTriggered { get; set; }

        public string GuaranteedLossStatus { get; set; }

        public bool GuaranteedLossTriggered { get; set; }
    }

    public class BetDetail
    {
        public int ChoiceId { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal ValidBetAmount { get; set; }

        public decimal TotalWin { get; set; }
    }
}
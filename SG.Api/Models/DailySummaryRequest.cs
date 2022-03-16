using System;
using SG.Core.Shared.Models;

namespace SG.Api.Models
{
    public class DailySummaryRequest : OperatorRequest
    {
        public Auth Auth { get; set; }

        public DateTimeOffset SummarizedDate { get; set; }
    }
}
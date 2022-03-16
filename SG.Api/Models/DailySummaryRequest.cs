using System;

namespace SG.Api.Models
{
    public class DailySummaryRequest
    {
        public Auth Auth { get; set; }

        public string Seq { get; set; }

        public DateTimeOffset SummarizedDate { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using SG.Core.Shared.Models;

namespace SG.Api.Models
{
    public class DailySummaryRequest : OperatorRequest
    {
        public Auth Auth { get; set; }

        [Required]
        public DateTimeOffset SummarizedDate { get; set; }
    }
}
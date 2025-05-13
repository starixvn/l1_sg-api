using System;
using SG.Core.Shared.Models;

namespace SG.Core.Features.Promotions
{
    public class PromotionResult : SGResult
    {
        public string PromotionCode { get; set; }
        
        public string PromotionName { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public int TotalBetCount { get; set; }
        
        public PromotionStatus Status { get; set; }
        
        public DateTimeOffset EndTime { get; set; }
    }
} 
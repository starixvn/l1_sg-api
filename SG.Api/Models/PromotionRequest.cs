using System;
using System.ComponentModel.DataAnnotations;
using SG.Core.Shared.Models;

namespace SG.Api.Models
{
    public class PromotionRequest : OperatorRequest
    {
        public DateTimeOffset TimeStamp { get; set; }
        
        [Required]
        public Auth Auth { get; set; }
        
        [Required]
        public string PromotionCode { get; set; }
    }
} 
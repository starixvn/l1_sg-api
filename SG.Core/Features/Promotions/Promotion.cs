using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SG.Core.Features.Promotions
{
    public class Promotion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("Code")]
        public string PromotionCode { get; set; }
        
        [BsonElement("Name")]
        public string PromotionName { get; set; }
        
        public string OperatorId { get; set; }
        
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalAmount { get; set; }
        
        public int TotalBetCount { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTimeOffset StartDate { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTimeOffset EndDate { get; set; }
  
        public PromotionStatus GetStatus()
        {
            var now = DateTimeOffset.UtcNow;
            
            return now switch
            {
                var d when d < StartDate => PromotionStatus.Upcoming,
                var d when d < EndDate => PromotionStatus.Running,
                _ => PromotionStatus.Completed
            };
        }
    }
} 
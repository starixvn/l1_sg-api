using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SG.Core.Features.DailySummaries
{
    public class DailySummary
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Time { get; set; }

        public int GameId { get; set; }

        public string PartnerId { get; set; }

        public string Currency { get; set; }

        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public decimal Stake { get; set; }

        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public decimal Payout { get; set; }

        public int Tickets { get; set; }
    }
}
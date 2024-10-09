using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SG.Core.Features.Operators
{
    public class CurrencyGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }
        
        public string PartnerId { get; set; }

        public bool FractionalOddsFormat { get; set; }
    }
}
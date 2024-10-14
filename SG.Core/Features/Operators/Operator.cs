using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SG.Core.Features.Operators
{
    public class Operator
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string PartnerId { get; set; }

        public string SecretKey { get; set; }

        public string MiniVersionUrl { get; set; }

        public string MobiVersionUrl { get; set; }

        public string ClubVersionUrl { get; set; }

        public string MegaKenoUrl { get; set; }

        public string MobileDirectUrl { get; set; }

        public string TestCurrencyUrl { get; set; }

        public string Alias { get; set; }
    }
}
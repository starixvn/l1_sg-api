using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SG.Core.Features.User
{
    public class User
    {
        [BsonElement("local")]
        public LocalEntity Local { get; set; }

        public string PlayerId { get; set; }

        public string OperatorId { get; set; }

        public string OperatorPlayerId { get; set; }

        public bool? IsInternal { get; set; }
    }

    public class LocalEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime RegDate { get; set; }
        public string ClientId { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public string Currency { get; set; }
        public string OperatorId { get; set; }
    }
}

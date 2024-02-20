using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SG.Core.Features.MemberAuthentications
{
    public class MemberAuth
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }  

        public string IP { get; set; }

        public string Domain { get; set; }

        public string BrowserUserAgent { get; set; }

        public string Token { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTimeOffset Time { get; set; }

        public string Language { get; set; }

        public string PlayerId { get; set; }

        public string PlayerName { get; set; }

        public string SiteId { get; set; }

        public string CurrencyName { get; set; }

        public string Seq { get; set; }

        public string OperatorId { get; set; }

        public byte AppType { get; set; }

        public string SiteName { get; set; }

        public string CurrencyCode { get; set; }

        public string CurrencyGroupId { get; set; }

        public double Multiplier { get; set; }

        public bool IsTestCurrency { get; set; }

        public string OperatorToken { get; set; }

        public bool DisableJackpotGames { get; set; }

        public bool IsCash { get; set; }

        public string Ref { get; set; }

        public string ReturnUrl { get; set; }

        public string OperatorPlayerId { get; set; }

#pragma warning disable S3874 // "out" and "ref" parameters should not be used
        public void Deconstruct(out string token, out bool isTestCurrency, out string partnerId)
#pragma warning restore S3874 // "out" and "ref" parameters should not be used
        {
            token = Token;
            isTestCurrency = IsTestCurrency;
            partnerId = OperatorId;
        }
    }
}
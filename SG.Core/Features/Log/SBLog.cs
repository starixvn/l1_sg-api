using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SG.Core.Shared.Models;

namespace SG.Core.Features.Log
{
    public class SBLog
    {
        public SBLog(OperatorRequest request, SGResult result, DateTimeOffset requestTimeStamp, DateTimeOffset responseTimeStamp)
        {
            LogTimeStamp = DateTimeOffset.UtcNow;
            RequestTimeStamp = requestTimeStamp;
            Request = request;
            ResponseTimeStamp = responseTimeStamp;
            Result = result;
        }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset LogTimeStamp { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset RequestTimeStamp { get; set; }

        public OperatorRequest Request { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset ResponseTimeStamp { get; set; }

        public SGResult Result { get; set; }
    }
}
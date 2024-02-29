using Newtonsoft.Json;

namespace SG.Core.Features.EndRoundInfo
{
    public class EndRoundInfoRequest
    {
        public EndRoundInfoRequest(int gameRoundId, int gameTypeId, string playerId, string operatorId, string operatorSecret, string siteId, bool? freeBet)
        {
            GameRoundId = gameRoundId;
            GameTypeId = gameTypeId;
            PlayerId = playerId;
            Auth = new Auth
            {
                OperatorId = operatorId,
                OperatorSecret = operatorSecret
            };
            FreeBet = freeBet;
            SiteId = siteId;
        }

        public int GameRoundId { get; }

        public int GameTypeId { get; }

        public string PlayerId { get; }

        public string SiteId { get; }

        public Auth Auth { get; }

#pragma warning disable CA1507 // Use nameof to express symbol names
        [JsonProperty("FreeBet", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning restore CA1507 // Use nameof to express symbol names
        public bool? FreeBet { get; }
    }

    public class Auth
    {
        public string OperatorId { get; set; }
        public string OperatorSecret { get; set; }
    }
}
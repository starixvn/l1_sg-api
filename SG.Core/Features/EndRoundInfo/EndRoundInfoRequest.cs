using Newtonsoft.Json;

namespace SG.Core.Features.EndRoundInfo
{
    public class EndRoundInfoRequest
    {
        public EndRoundInfoRequest(int gameRoundId, byte gameTypeId, int playerId, string operatorId, string operatorSecret, bool? freeBet)
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
        }

        public int GameRoundId { get; }

        public byte GameTypeId { get; }

        public int PlayerId { get; }

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
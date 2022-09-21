namespace SG.Core.Features.EndRoundInfo
{
    public class EndRoundInfoRequest
    {
        public EndRoundInfoRequest(int gameRoundId, byte gameTypeId, int playerId, string siteId, string operatorId, string operatorSecret, bool? freeBet)
        {
            GameRoundId = gameRoundId;
            GameTypeId = gameTypeId;
            PlayerId = playerId;
            SiteId = siteId;
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

        public bool? FreeBet { get; }
    }

    public class Auth
    {
        public string OperatorId { get; set; }
        public string OperatorSecret { get; set; }
    }
}
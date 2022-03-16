namespace SG.Core.Features.EndRoundInfo
{
    public class EndRoundInfoRequest
    {
        public EndRoundInfoRequest(int gameRoundId, byte gameTypeId, int playerId, string siteId, string partnerId, bool freeBet)
        {
            GameRoundId = gameRoundId;
            GameTypeId = gameTypeId;
            PlayerId = playerId;
            SiteId = siteId;
            OperatorId = partnerId;
            FreeBet = freeBet;
        }

        public int GameRoundId { get; }

        public byte GameTypeId { get; }

        public int PlayerId { get; }

        public string SiteId { get; }

        public string OperatorId { get; }

        public bool FreeBet { get; }
    }
}
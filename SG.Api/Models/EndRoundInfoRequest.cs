using SG.Core.Shared.Models;

namespace SG.Api.Models
{
    public class EndRoundInfoRequest : OperatorRequest
    {
        public Auth Auth { get; set; }

        public int GameRoundId { get; set; }

        public byte GameTypeId { get; set; }

        public int PlayerId { get; set; }

        public string SiteId { get; set; }

        public bool FreeBet { get; set; }
    }
}
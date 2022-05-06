using System.ComponentModel.DataAnnotations;
using SG.Core.Shared.Models;

namespace SG.Api.Models
{
    public class EndRoundInfoRequest : OperatorRequest
    {
        public Auth Auth { get; set; }

        [Required]
        public int GameRoundId { get; set; }

        [Required]
        public byte GameTypeId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        public bool FreeBet { get; set; }
    }
}
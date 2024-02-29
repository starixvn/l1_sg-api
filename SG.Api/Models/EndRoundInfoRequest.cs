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
        public int GameTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[^~]+$")]
        public string PlayerId { get; set; }

        public bool FreeBet { get; set; }
    }
}
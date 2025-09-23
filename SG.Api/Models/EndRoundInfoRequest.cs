using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SG.Api.Extensions;
using SG.Core.Shared.Models;

namespace SG.Api.Models
{
    public class EndRoundInfoRequest : OperatorRequest
    {
        public Auth Auth { get; set; }

        [Required]
        public long GameRoundId { get; set; }

        [Required]
        public int GameTypeId { get; set; }

        [Required]
        [JsonConverter(typeof(PlayerIdConverter))]
        public object PlayerId { get; set; }

        public bool FreeBet { get; set; }
    }
}
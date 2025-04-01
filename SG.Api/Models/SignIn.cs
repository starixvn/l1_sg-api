using System.Text.Json.Serialization;
using SG.Api.Extensions;
using SG.Core.Shared.Constants;
using SG.Core.Shared.Models;

namespace SG.Api.Models
{
    public class SignIn : OperatorRequest
    {
        public string Domain { get; set; }

        public string BrowserUserAgent { get; set; }

        public string Currency { get; set; }

        public string Language { get; set; } = "en-US";

        public string IP { get; set; }

        [JsonConverter(typeof(PlayerIdConverter))]
        public object PlayerId { get; set; }

        public string PlayerName { get; set; }

        public int? GameTypeId { get; set; }

        public byte AppType { get; set; } = AppTypes.SabaClub;

        public string SiteName { get; set; }

        public Auth Auth { get; set; }

        public string OperatorToken { get; set; }

        public bool DisableJackpotGames { get; set; }

        public bool IsCash { get; set; }

        public string Ref { get; set; }

        public string ReturnUrl { get; set; }

        public bool DisableAward { get; set; }
    }
}
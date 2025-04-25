using System.ComponentModel.DataAnnotations;

namespace SG.Api.Models
{
    public class UpdateMemberAuthRequest : Auth
    {
        [Required]
        public string Token { get; set; }

        public int? GameTypeId { get; set; }

        public string CurrencyGroupId { get; set; }
    }
}
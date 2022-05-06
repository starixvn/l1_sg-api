using System.ComponentModel.DataAnnotations;

namespace SG.Api.Models
{
    public class Auth
    {
        [Required]
        public string OperatorId { get; set; }
    }
}
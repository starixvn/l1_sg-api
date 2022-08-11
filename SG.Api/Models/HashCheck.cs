using System.ComponentModel.DataAnnotations;

namespace SG.Api.Models
{
    public class HashCheck
    {
        [Required]
        public string SecretKey { get; set; }

        [Required]
        public string Data { get; set; }
    }
}
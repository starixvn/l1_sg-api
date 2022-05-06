using SG.Core.Shared.Constants;

namespace SG.Core.Shared.Models
{
    public class SGResult
    {
        public string ErrorCode { get; set; } = ErrorCodes.SuccessCode;

        public string ErrorDescription { get; set; }

        public string Seq { get; set; }
    }
}
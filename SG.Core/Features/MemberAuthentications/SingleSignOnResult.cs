using System;
using SG.Core.Shared.Models;

namespace SG.Core.Features.MemberAuthentications
{
    public class SingleSignOnResult : SGResult
    {


        public SingleSignOnResult(int code, string message, Uri redirectUri = null)
        {
            Code = code;
            Message = message;
            RedirectUri = redirectUri;
            RedirectUrl = RedirectUri == null ? string.Empty : RedirectUri.ToString();
        }

        public const int SuccessCode = 0;
        public const int ClientIdDoesNotExistCode = 1;
        public const int WrongClientSecretCode = 2;
        public const int ApiServerNotExistCode = 3;
        public const int IdentityServerNotExistCode = 4;
        public const int ServerErrorCode = 5;
        public const int GameServerNotFoundCode = 6;
        public const int Unauthorize = 7;


        public bool IsSuccessful => Code == SuccessCode;

        public static SingleSignOnResult ServerErrorResult => new(ServerErrorCode, string.Empty);

        public Uri RedirectUri { get; }

        public string RedirectUrl { get; set; }

        public int Code { get; }

        public string Message { get; }

        public static SingleSignOnResult CreateSuccessResponse(string redirectUrl)
            => new(SuccessCode, string.Empty, new Uri(redirectUrl));

        public static SingleSignOnResult CreateErrorResponse(int code)
            => new(code, string.Empty);
    }
}
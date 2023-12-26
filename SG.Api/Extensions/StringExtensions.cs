using SG.Core.Shared.Constants;

namespace SG.Api.Extensions
{
    public static class StringFormat
    {
        public static string FormatPlayerId(string operatorId, string siteId, string playerId)
        {
            if (operatorId == Operators.SabaPlay)
            {
                return string.IsNullOrWhiteSpace(siteId) ? playerId : $"{siteId}~{playerId}";
            }

            return playerId;
        }
    }
}
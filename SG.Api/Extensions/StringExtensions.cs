namespace SG.Api.Extensions
{
    public static class StringFormat
    {
        public static string FormatPlayerId(string siteId, string playerId)
        {
            return string.IsNullOrWhiteSpace(siteId) ? playerId : $"{siteId}~{playerId}";
        }
    }
}
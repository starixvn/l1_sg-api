namespace SG.Api.Extensions
{
    public static class StringFormat
    {
        public static string FormatPlayerId(string siteId, string operatorId)
        {
            return $"{siteId}~{operatorId}";
        }
    }
}
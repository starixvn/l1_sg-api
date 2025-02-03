namespace SG.Core.Shared.Utils
{
    public static class StringFormat
    {  
        public static string FormatUserName(string operatorId, string playerId)
        {
             return $"{operatorId}_{playerId}";         
        }
    }
}
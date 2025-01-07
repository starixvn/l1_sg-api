namespace SG.Core.Shared.Constants
{
    public static class MongoDatabases
    {
        const string Member = "member";
        const string Log = "log";
        const string Minigame = "minigame";
        const string Summary = "summary";

        public static string FormatDatabaseName(string dbName, string prefix) 
            => string.IsNullOrWhiteSpace(prefix) ? dbName : prefix + "_" + dbName;

        public static string GetMemberDatabase(string prefix) 
            => FormatDatabaseName(Member, prefix);

        public static string GetLogDatabase(string prefix)
            => FormatDatabaseName(Log, prefix);

        public static string GetMinigameDatabase(string prefix)
            => FormatDatabaseName(Minigame, prefix);

        public static string GetSummaryDatabase(string prefix)
            => FormatDatabaseName(Summary, prefix);
    }
}
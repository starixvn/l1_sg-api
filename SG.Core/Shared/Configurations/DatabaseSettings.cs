namespace SG.Core.Shared.Configurations
{
    public interface IDatabaseSettings
    {
        string ConnectionString { get; set; }

        string PrefixDbName { get; set; }
    }

    public class DatabaseSettings : IDatabaseSettings
    {
        public string ConnectionString { get; set; }

        public string PrefixDbName { get; set; }
    }
}
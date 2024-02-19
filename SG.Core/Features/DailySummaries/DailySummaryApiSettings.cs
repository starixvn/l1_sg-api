namespace SG.Core.Features.DailySummaries
{
    public class DailySummaryApiSettings
    {
        public bool IsCloud { get; set; }

        public string SGApiCloudUrl { get; set; }

        public bool EnableCloud { get; set; }

        public string ProxyUrl { get; set; }
    }
}
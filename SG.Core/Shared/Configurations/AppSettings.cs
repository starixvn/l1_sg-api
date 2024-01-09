using System.Collections.Generic;
using SG.Core.Features.DailySummaries;
using SG.Core.Features.ResultApiCommunicators;

namespace SG.Core.Shared.Configurations
{
    public interface IAppSettings
    {
        int TokenExpiredSeconds { get; set; }

        ResultApiSettings ResultApiSettings { get; set; }

        DailySummaryApiSettings DailySummaryApiSettings { get; set; }

        Dictionary<string, string> DailySummaryConsolidationMapping { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        public int TokenExpiredSeconds { get; set; }

        public ResultApiSettings ResultApiSettings { get; set; }

        public Dictionary<string, string> DailySummaryConsolidationMapping { get; set; } = new Dictionary<string, string>();

        public DailySummaryApiSettings DailySummaryApiSettings { get; set; }
    }
}
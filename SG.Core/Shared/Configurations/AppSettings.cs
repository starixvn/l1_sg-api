using SG.Core.Features.ResultApiCommunicators;

namespace SG.Core.Shared.Configurations
{
    public interface IAppSettings
    {
        int TokenExpiredSeconds { get; set; }

        ResultApiSettings ResultApiSettings { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        public int TokenExpiredSeconds { get; set; }

        public ResultApiSettings ResultApiSettings { get; set; }
    }
}
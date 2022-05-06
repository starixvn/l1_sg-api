using System.Threading.Tasks;

namespace SG.Core.Features.Log
{
    public interface ILogService
    {
        Task LogRetrieveEndRoundAsync(SBLog log);
    }

    public class LogService : ILogService
    {
        private readonly ILogDataAccess logDataAccess;

        public LogService(ILogDataAccess logDataAccess)
        {
            this.logDataAccess = logDataAccess;
        }

        public Task LogRetrieveEndRoundAsync(SBLog log)
            => logDataAccess.WriteRetrieveEndRoundAsync(log);
    }
}
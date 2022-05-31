using System.Threading.Tasks;

namespace SG.Core.Features.Log
{
    public interface ILogService
    {
        Task LogRetrieveEndRoundAsync(SBLog log);
        Task LogSignInAsync(SBLog log);
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

        public Task LogSignInAsync(SBLog log)
           => logDataAccess.WriteSignInAsync(log);
    }
}
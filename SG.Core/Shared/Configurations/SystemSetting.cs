using System.Collections.Generic;

namespace SG.Core.Shared.Configurations
{
    public interface ISystemSetting
    {
        public string SystemType { get; }
        public IEnumerable<string> DisableJackpotOperators { get; }
    }

    public class SystemSetting : ISystemSetting
    {
        public SystemSetting(string systemType, IEnumerable<string> disableJackpotOperators)
        {
            SystemType = systemType;
            DisableJackpotOperators = disableJackpotOperators;
        }

        public string SystemType { get; }
        public IEnumerable<string> DisableJackpotOperators { get; }
    }
}

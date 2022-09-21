namespace SG.Core.Shared.Configurations
{
    public interface ISystemSetting
    {
        public string SystemType { get; }
    }

    public class SystemSetting : ISystemSetting
    {
        public SystemSetting(string systemType)
        {
            SystemType = systemType;
        }

        public string SystemType { get; }
    }
}

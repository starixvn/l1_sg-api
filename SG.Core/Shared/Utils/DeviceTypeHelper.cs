using System.Collections.Generic;

namespace SG.Core.Shared.Utils
{
    public static class DeviceTypeHelper
    {
        public static DeviceType GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return DeviceType.Unknown;
            }

            userAgent = userAgent.ToLowerInvariant();
            
            // Mobile devices
            var mobileAgents = new List<string> {"mobile", "android", "iphone", "ipad", "ipod", "windows phone", "blackberry"};
            if (mobileAgents.Exists(agent => userAgent.Contains(agent)))
            {
                return DeviceType.Mobile;
            }
    
            // Desktop devices
            var desktopAgents = new List<string> {"windows", "macintosh", "linux", "x11"};
            if (desktopAgents.Exists(agent => userAgent.Contains(agent)))
            {
                return DeviceType.Desktop;
            }

            return DeviceType.Unknown;
        }
    }

    public enum DeviceType
    {
        Unknown,
        Mobile,
        Desktop
    }
}
namespace SG.Core.Shared.Utils
{
    public static class DeviceTypeHelper
    {
        public static DeviceType GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return DeviceType.Unknown;

            userAgent = userAgent.ToLower();

            // Mobile devices
            if (userAgent.Contains("mobile") ||
                userAgent.Contains("android") ||
                userAgent.Contains("iphone") ||
                userAgent.Contains("ipad") ||
                userAgent.Contains("ipod") ||
                userAgent.Contains("windows phone") ||
                userAgent.Contains("blackberry"))
            {
                return DeviceType.Mobile;
            }

            // Desktop devices
            if (userAgent.Contains("windows") ||
                userAgent.Contains("macintosh") ||
                userAgent.Contains("linux") ||
                userAgent.Contains("x11"))
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
using SG.Core.Shared.Utils;
using Xunit;

namespace SG.Core.Tests.Shared
{
    public class DeviceTypeHelperTests
    {
        [Theory]
        [InlineData(null, DeviceType.Unknown)]
        [InlineData("", DeviceType.Unknown)]
        [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15", DeviceType.Mobile)]
        [InlineData("Mozilla/5.0 (iPad; CPU OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15", DeviceType.Mobile)]
        [InlineData("Mozilla/5.0 (iPod; CPU iPhone OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15", DeviceType.Mobile)]
        [InlineData("Mozilla/5.0 (Linux; Android 11; SM-G991B) AppleWebKit/537.36", DeviceType.Mobile)]
        [InlineData("Mozilla/5.0 (Windows Phone 10.0; Android 6.0.1; Microsoft; Lumia 950) AppleWebKit/537.36", DeviceType.Mobile)]
        [InlineData("Mozilla/5.0 (BlackBerry; U; BlackBerry 9900; en) AppleWebKit/534.11+", DeviceType.Mobile)]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36", DeviceType.Desktop)]
        [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36", DeviceType.Desktop)]
        [InlineData("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36", DeviceType.Desktop)]
        [InlineData("Mozilla/5.0 (Unknown; Unknown) AppleWebKit/537.36", DeviceType.Unknown)]
        public void GetDeviceType_WithVariousUserAgents_ReturnsCorrectDeviceType(string userAgent, DeviceType expectedDeviceType)
        {
            // Act
            var result = DeviceTypeHelper.GetDeviceType(userAgent);

            // Assert
            Assert.Equal(expectedDeviceType, result);
        }

        [Theory]
        [InlineData("MOBILE", DeviceType.Mobile)]
        [InlineData("ANDROID", DeviceType.Mobile)]
        [InlineData("IPHONE", DeviceType.Mobile)]
        [InlineData("IPAD", DeviceType.Mobile)]
        [InlineData("IPOD", DeviceType.Mobile)]
        [InlineData("WINDOWS PHONE", DeviceType.Mobile)]
        [InlineData("BLACKBERRY", DeviceType.Mobile)]
        [InlineData("WINDOWS", DeviceType.Desktop)]
        [InlineData("MACINTOSH", DeviceType.Desktop)]
        [InlineData("LINUX", DeviceType.Desktop)]
        [InlineData("X11", DeviceType.Desktop)]
        public void GetDeviceType_WithCaseInsensitiveUserAgents_ReturnsCorrectDeviceType(string userAgent, DeviceType expectedDeviceType)
        {
            // Act
            var result = DeviceTypeHelper.GetDeviceType(userAgent);

            // Assert
            Assert.Equal(expectedDeviceType, result);
        }

        [Theory]
        [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15", DeviceType.Mobile)]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36", DeviceType.Desktop)]
        [InlineData("Mozilla/5.0 (Unknown; Unknown) AppleWebKit/537.36", DeviceType.Unknown)]
        public void GetDeviceType_WithMixedCaseUserAgents_ReturnsCorrectDeviceType(string userAgent, DeviceType expectedDeviceType)
        {
            // Act
            var result = DeviceTypeHelper.GetDeviceType(userAgent);

            // Assert
            Assert.Equal(expectedDeviceType, result);
        }
    }
}
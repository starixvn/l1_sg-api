using System.Linq;

namespace SG.Core.Shared.Constants
{
    public static class AppTypes
    {
        public const byte SabaClub = 1;
        public const byte SabaMobi = 2;
        public const byte MiniSabaClub = 3;
        public const byte MegaKeno = 4;
        public const byte SabaApp = 5;
        public const byte DesktopSabaDirect = 6;
        public const byte MobileSabaDirect = 7;

        private static readonly byte[] appTypes = { SabaClub, SabaMobi, MiniSabaClub, MegaKeno, SabaApp, DesktopSabaDirect, MobileSabaDirect };

        public static bool IsValid(byte appType)
        {
            return appTypes.Any(x => x == appType);
        }
    }
}
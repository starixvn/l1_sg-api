using System;

namespace SG.Core.Shared.Constants
{
    public static class AppTypes
    {
        public const byte SabaClub = 1;
        public const byte SabaMobi = 2;
        public const byte MiniSabaClub = 3;
        public const byte MegaKeno = 4;
        public const byte SabaApp = 5;
        public const byte DesktopDirect = 6;
        public const byte MobileDirect = 7;
        public const byte TodayPageDesktopDirect = 8;
        public const byte Langlive = 9;

        private static readonly byte[] appTypes = { SabaClub, SabaMobi, MiniSabaClub, MegaKeno, SabaApp, DesktopDirect, MobileDirect, TodayPageDesktopDirect, Langlive };

        public static bool IsValid(byte appType)
        {
            return Array.Exists(appTypes, x => x == appType);
        }
    }
}
namespace SG.Core.Shared.Constants
{
    public static class AppTypes
    {
        public const byte SabaClub = 1;
        public const byte SabaMobi = 2;
        public const byte MiniSabaClub = 3;
        public const byte MegaKeno = 4;
        public const byte SabaApp = 5;

        public static bool IsValid(byte appType)
        {
            return appType == SabaClub || appType == SabaMobi || appType == MiniSabaClub || appType == MegaKeno || appType == SabaApp;
        }
    }
}
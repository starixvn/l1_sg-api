namespace SG.Core.Shared.Constants
{
    public static class ViewModes
    {
        public const byte Landscape = 1;
        public const byte Portrait = 2;

        public static bool IsPortrait(byte viewMode)
        {
            return viewMode == Portrait;
        }
    }
}

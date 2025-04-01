namespace SG.Core.Features.GameConfigurations
{
    public class GameConfigurationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public bool? IsMicrosite { get; set; }
        public bool? IsOpenDirectly { get; set; }

        public bool? IsSupportedPortraitMode { get; set; }
        public GameConfigurationGateway Gateway { get; set; }
    }

    public class GameConfigurationGateway
    {
        public int Id { get; set; }
    }
}

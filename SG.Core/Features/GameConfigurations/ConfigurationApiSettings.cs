using System;

namespace SG.Core.Features.GameConfigurations
{
    public class ConfigurationApiSettings
    {
        public Uri BaseUrl { get; set; }
        public ConfigurationApiEndPoints Endpoints { get; set; }
    }

    public class ConfigurationApiEndPoints
    {
        public string DetailGameConfiguration { get; set; }
    }
}

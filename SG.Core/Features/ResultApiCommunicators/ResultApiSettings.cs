using System;

namespace SG.Core.Features.ResultApiCommunicators
{
    public class ResultApiSettings
    {
        public Uri BaseUrl { get; set; }

        public ResultApiEndPoints Endpoints { get; set; }
    }

    public class ResultApiEndPoints
    {
        public string EndRoundInfo { get; set; }
    }
}
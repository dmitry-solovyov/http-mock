using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public struct ConfigurationDefinition
    {
        public string? Info { get; set; }

        public List<RequestConfigurationDefinition>? Map { get; set; }

        public bool HasData()
        {
            return Map?.Count > 0;
        }
    }

    public struct RequestConfigurationDefinition
    {
        public string? Url { get; set; }

        public string? Description { get; set; }

        public string? Method { get; set; }

        public int? Status { get; set; }

        public int? Delay { get; set; }

        public string? Payload { get; set; }

        public Dictionary<string, string>? Headers { get; set; }
    }
}

using Newtonsoft.Json;

namespace Sentry.data.Core
{
    public class ConnectorCreateRequestDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("config")]
        public ConnectorCreateRequestConfigDto Config { get; set; }

    }
}

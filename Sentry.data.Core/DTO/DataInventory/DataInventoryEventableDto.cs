using Newtonsoft.Json;

namespace Sentry.data.Core
{
    public abstract class DataInventoryEventableDto
    {
        [JsonIgnore]
        public DataInventoryEventDto DataInventoryEvent { get; set; }
    }
}

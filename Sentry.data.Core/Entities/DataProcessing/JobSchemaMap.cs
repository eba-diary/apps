namespace Sentry.data.Core.Entities.DataProcessing
{
    public class JobSchemaMap
    {
        public DataFlow FlowId { get; set; }
        public Schema MappedSchema { get; set; }
        public string SearchCriteria { get; set; }
    }
}

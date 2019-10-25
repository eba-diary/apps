namespace Sentry.data.Core.Entities.DataProcessing
{
    public class SchemaMap
    {
        public virtual int Id { get; set; }
        public virtual DataFlowStep DataFlowStepId { get; set; }
        public virtual FileSchema MappedSchema { get; set; }
        public virtual string SearchCriteria { get; set; }
    }
}

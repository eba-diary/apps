namespace Sentry.data.Core
{
    public class SchemaMapDto
    {
        public int Id { get; set; }
        public int StepId { get; set; }
        public int SchemaId { get; set; }
        public int DatasetId { get; set; }
        public string SearchCriteria { get; set; }
        public bool IsDeleted { get; set; }
    }
}

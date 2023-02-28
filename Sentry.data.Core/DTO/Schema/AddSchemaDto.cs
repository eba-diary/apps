namespace Sentry.data.Core
{
    public class AddSchemaDto
    {
        public FileSchemaDto SchemaDto { get; set; }
        public DatasetFileConfigDto DatasetFileConfigDto { get; set; }
        public DataFlowDto DataFlowDto { get; set; }
    }
}

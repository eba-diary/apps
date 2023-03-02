namespace Sentry.data.Core
{
    public class SchemaFlowDto : IIdentifiableDto
    {
        public FileSchemaDto SchemaDto { get; set; }
        public DatasetFileConfigDto DatasetFileConfigDto { get; set; }
        public DataFlowDto DataFlowDto { get; set; }

        public void SetId(int id)
        {
            SchemaDto.SchemaId = id;
        }
    }
}

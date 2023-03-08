namespace Sentry.data.Web.Models.ApiModels.Migration
{
    public class DatasetSchemaMigrationRequestModel
    {
        public int SourceSchemaId { get; set; }
        public string TargetDataFlowNamedEnviornment { get; set; }
    }
}
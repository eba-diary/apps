namespace Sentry.data.Core
{
    public class DatasetSchemaMigrationRequest
    {
        public int SourceSchemaId { get; set; }
        public string TargetDataFlowNamedEnvironment { get; set; }

        public static class ValidationErrors
        {
            public const string SourceSchemaIdRequired = "SourceSchemaId is required";
            public const string TargetDataFlowNamedEnvironmentRequired = "TargetDataFlowNamedEnvironment is required";
            public const string TargetDataFlowNamedEnvironmentIsInvalid = "TargetDataFlowNamedEnvironment is invalid";
            public const string DatasetsAreNotRelated = "Target datasets are not related";
        }
    }
}

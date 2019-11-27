using System.ComponentModel;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public enum DataActionType
    {
        None = 0,
        [Description("S3 Drop")]
        S3Drop = 1,
        [Description("Raw Storage")]
        RawStorage = 2,
        [Description("Query Storage")]
        QueryStorage = 3,
        [Description("Schema Load")]
        SchemaLoad = 4,
        [Description("Convert to Parquet")]
        ConvertParquet = 5
    }
}

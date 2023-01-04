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
        ConvertParquet = 5,
        [Description("Uncompress Zip")]
        UncompressZip = 6,
        [Description("Uncompress GZip")]
        UncompressGzip = 7,
        [Description("Schema Map")]
        SchemaMap = 8,
        [Description("Google Api")]
        GoogleApi = 9,
        [Description("ClaimIQ")]
        ClaimIq = 10,
        [Description("Fixed Width")]
        FixedWidth = 11,
        [Description("Producer S3 Drop")]
        ProducerS3Drop = 12,
        [Description("XML")]
        XML = 13,
        [Description("JSONFlattening")]
        JsonFlattening = 14,
        [Description("Google BigQuery API")]
        GoogleBigQueryApi = 15,
        [Description("Google Search Console API")]
        GoogleSearchConsoleApi = 16
    }
}

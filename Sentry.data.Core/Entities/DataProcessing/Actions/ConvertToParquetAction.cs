namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConvertToParquetAction : BaseAction
    {
        public ConvertToParquetAction()
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX;
        }        
    }
}

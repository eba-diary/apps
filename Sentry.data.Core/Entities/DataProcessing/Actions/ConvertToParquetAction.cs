namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConvertToParquetAction : BaseAction
    {
        public ConvertToParquetAction()
        {
            TargetStoragePrefix = "parquet/";
        }        
    }
}

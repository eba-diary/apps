using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConvertToParquetAction : BaseAction
    {
        private IConvertToParquetProvider _convertToParquetProvider;
        public ConvertToParquetAction() { }

        public ConvertToParquetAction(IConvertToParquetProvider convertToParquetProvider) : base(convertToParquetProvider)
        {
            _convertToParquetProvider = convertToParquetProvider;
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX;
        }
    }
}

using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConvertToParquetAction : BaseAction
    {
        public ConvertToParquetAction() { }

        public ConvertToParquetAction(IConvertToParquetProvider convertToParquetProvider) : base(convertToParquetProvider) { }
    }
}

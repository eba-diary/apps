using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class CopyToParquetAction : BaseAction
    {
        public CopyToParquetAction() { }

        public CopyToParquetAction(IBaseActionProvider actionProvider) : base(actionProvider) { }
    }
}

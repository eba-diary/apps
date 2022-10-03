using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class GoogleBigQueryApiAction : BaseAction
    {
        public GoogleBigQueryApiAction() { }
        public GoogleBigQueryApiAction(IBaseActionProvider actionProvider) : base(actionProvider) { }
    }
}

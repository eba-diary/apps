using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class GoogleSearchConsoleApiAction : BaseAction
    {
        public GoogleSearchConsoleApiAction() { }
        public GoogleSearchConsoleApiAction(IBaseActionProvider actionProvider) : base(actionProvider) { }
    }
}

using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class FixedWidthAction : BaseAction
    {
        public FixedWidthAction() { }

        public FixedWidthAction(IFixedWidthProvider fixedWitdhProvider) : base(fixedWitdhProvider) { }
    }
}

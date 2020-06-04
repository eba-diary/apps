using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class UncompressGzipAction : BaseAction
    {
        public UncompressGzipAction() { }

        public UncompressGzipAction(IUncompressGzipProvider uncompressGzipProvider) : base(uncompressGzipProvider) { }
    }
}

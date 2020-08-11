using Sentry.data.Core.Interfaces.DataProcessing;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class UncompressZipAction : BaseAction
    {
        public UncompressZipAction() { }

        public UncompressZipAction(IUncompressZipProvider uncompressZipProvider) : base(uncompressZipProvider) { }
    }
}

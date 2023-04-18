using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class GetByEnvironmentDatasetIdResult
    {
        public GlobalDataset GlobalDataset { get; set; }
        public EnvironmentDataset EnvironmentDataset { get; set; }

        public virtual bool WasFound()
        {
            return EnvironmentDataset != null;
        }
    }
}

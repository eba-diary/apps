using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class GetByEnvironmentSchemaIdResult : GetByEnvironmentDatasetIdResult
    {
        public EnvironmentSchema EnvironmentSchema { get; set; }

        public override bool WasFound()
        {
            return EnvironmentSchema != null;
        }
    }
}

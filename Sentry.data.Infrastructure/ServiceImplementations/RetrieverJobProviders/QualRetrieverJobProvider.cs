using Sentry.data.Core;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class QualRetrieverJobProvider : EnvironmentTypeRetrieverJobProvider
    {
        public QualRetrieverJobProvider(IDatasetContext datasetContext) : base(datasetContext) { }

        public override List<string> AcceptedNamedEnvironments => new List<string>() { DLPPEnvironments.QUAL, DLPPEnvironments.QUALNP };

        protected override bool IsProd(string requestingNamedEnvironment)
        {
            return !string.IsNullOrEmpty(requestingNamedEnvironment) && requestingNamedEnvironment.ToUpper() == DLPPEnvironments.QUAL;
        }
    }
}

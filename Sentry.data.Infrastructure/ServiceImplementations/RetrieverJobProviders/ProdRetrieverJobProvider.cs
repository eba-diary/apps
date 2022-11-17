using Sentry.data.Core;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class ProdRetrieverJobProvider : EnvironmentTypeRetrieverJobProvider
    {
        public ProdRetrieverJobProvider(IDatasetContext datasetContext) : base(datasetContext) { }

        public override List<string> AcceptedNamedEnvironments => new List<string>() { DLPPEnvironments.PROD, DLPPEnvironments.PRODNP };

        protected override bool IsProd(string requestingNamedEnvironment)
        {
            return string.IsNullOrEmpty(requestingNamedEnvironment) || requestingNamedEnvironment.ToUpper() == DLPPEnvironments.PROD;
        }
    }
}

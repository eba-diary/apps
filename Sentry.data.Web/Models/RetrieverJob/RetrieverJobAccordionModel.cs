using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class RetrieverJobAccordionModel
    {
        public RetrieverJobAccordionModel(string accordionId, NamedEnvironmentType datasetEnvironmentType, string cLA4260_QuartermasterNamedEnvironmentTypeFilter, List<RetrieverJob> retrieverJobs)
        {
            AccordionId = accordionId;
            DatasetEnvironmentType = datasetEnvironmentType;
            CLA4260_QuartermasterNamedEnvironmentTypeFilter = cLA4260_QuartermasterNamedEnvironmentTypeFilter;
            RetrieverJobs = retrieverJobs;
        }

        public string AccordionId { get; set; }
        public NamedEnvironmentType DatasetEnvironmentType { get; set; }
        public string CLA4260_QuartermasterNamedEnvironmentTypeFilter { get; set; }
        public List<RetrieverJob> RetrieverJobs { get; set; }
    }
}
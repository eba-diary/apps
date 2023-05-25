using Microsoft.Extensions.Logging;
using Sentry.data.Core.DependencyInjection;

using System.Linq;

namespace Sentry.data.Core.DomainServices.SampleService
{
    public class SampleDomainService : BaseDomainService<SampleDomainService>
    {
        private readonly IDatasetContext _datasetContext;

        public SampleDomainService(IDatasetContext datasetContext, DomainServiceCommonDependency<SampleDomainService> commonDependency) : base(commonDependency) 
        {
            _datasetContext = datasetContext;
        }

        public string SimpleLogStatement()
        {
            string datasetName = _datasetContext.Datasets.Take(1).Select(s => s.DatasetName).FirstOrDefault();
            _logger.LogInformation(datasetName);
            return datasetName;
        }
    }
}

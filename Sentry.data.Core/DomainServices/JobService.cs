using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class JobService : IJobService
    {
        private IDatasetContext _datasetConext;

        public JobService(IDatasetContext datasetContext)
        {
            _datasetConext = datasetContext;
        }

        public JobHistory GetLastExecution(RetrieverJob job)
        {
            return _datasetConext.JobHistory.OrderByDescending(o => o.Created).Take(1).SingleOrDefault(w => w.JobId.Id == job.Id && w.State == GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE);
        }
    }
}

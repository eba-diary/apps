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
            return _datasetConext.JobHistory.Where(w => w.JobId.Id == job.Id && w.State == GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE).OrderByDescending(o => o.Created).Take(1).SingleOrDefault();
        }

        public void RecordJobState(Submission submission, RetrieverJob job, string state)
        {
            JobHistory histRecord = null;

            if (job.DataSource.Is<FtpSource>())
            {
                histRecord = new JobHistory()
                {
                    JobId = job,
                    BatchId = 0,
                    JobGuid = submission.JobGuid,
                    State = state,
                    LivyAppId = null,
                    LivyDriverLogUrl = null,
                    LivySparkUiUrl = null,
                    Active = true
                };
            }

            _datasetConext.Add(histRecord);
            _datasetConext.SaveChanges();
            
        }

        public Submission SaveSubmission(RetrieverJob job, string options)
        {
            Submission sub = job.ToSubmission();

            _datasetConext.Add(sub);
            _datasetConext.SaveChanges();

            return sub;
        }
    }
}

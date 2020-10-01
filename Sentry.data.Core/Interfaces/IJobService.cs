using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IJobService
    {
        JobHistory GetLastExecution(RetrieverJob job);

        /// <summary>
        /// Returns all submission records for a retriever job. If submission id is passed, 
        /// then specific submission will be returned.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="submissionId"></param>
        /// <returns></returns>
        /// <exception cref="Exceptions.JobNotFoundException">Thrown when retriever job not found</exception>
        List<Submission> GetJobSubmissions(int jobId, int submissionId = 0);
        List<JobHistory> GetJobHistoryBySubmission(int SubmissionId);
        List<JobHistory> GetJobHistoryByJobAndSubmission(int JobId, int SubmissionId);
        Submission SaveSubmission(RetrieverJob job, string options);
        void RecordJobState(Submission submission, RetrieverJob job, string state);
        RetrieverJob FindBasicJob(RetrieverJob job);
        RetrieverJob InstantiateJobsForCreation(DatasetFileConfig dfc, DataSource dataSource);
        RetrieverJob InstantiateJobsForCreation(DataFlow df, DataSource dataSource);
        RetrieverJob CreateAndSaveRetrieverJob(RetrieverJobDto dto);
        void CreateDropLocation(RetrieverJob job);
        void DisableJob(int id);
        void DeleteJob(int id);
        List<RetrieverJob> GetDfsRetrieverJobs();
    }
}

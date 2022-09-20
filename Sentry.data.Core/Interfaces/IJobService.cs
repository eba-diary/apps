using Sentry.data.Core.DTO.Job;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IJobService : IEntityService
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Retriever job identifier</param>
        void EnableJob(int id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Retriever job identifier</param>
        void DisableJob(int id);

        bool Delete(List<int> idList, IApplicationUser user, bool logicalDelete);

        List<RetrieverJob> GetDfsRetrieverJobs();

        Task<System.Net.Http.HttpResponseMessage> SubmitApacheLivyJobAsync(int JobId, System.Guid JobGuid, JavaOptionsOverrideDto dto);
        Task<System.Net.Http.HttpResponseMessage> GetApacheLibyBatchStatusAsync(int jobId, int batchId);
        Task<System.Net.Http.HttpResponseMessage> GetApacheLibyBatchStatusAsync(JobHistory historyRecord);
    }
}

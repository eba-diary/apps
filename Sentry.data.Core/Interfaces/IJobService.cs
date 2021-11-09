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
        /// <summary>
        /// Will perform Delete fore each job id provided.
        /// If deleteIssuerId is not specified, userService will be used to pull current user Id.
        /// </summary>
        /// <param name="idList">List of RetrieverJob identifiers</param>
        /// <param name="deleteIssuerId">If not supplied, user Id will be pulled from User service</param>
        /// <param name="logicalDelete">Perform soft or hard delete</param>
        void DeleteJob(List<int> idList, string deleteIssuerId = null, bool logicalDelete = true);

        /// <summary>
        /// Mark retriever job objectstatus based
        /// on logicalDelete flag: true = "Pending Delete", false = "Deleted".
        /// If deleteIssuerId is not specified, userService will be used to pull current user Id.
        /// </summary>
        /// <param name="id">RetrieverJob identifier</param>
        /// <param name="deleteIssuerId">If not supplied, will utilize userService to pull current user</param>
        /// <param name="logicalDelete">Perform soft or hard delete</param>
        /// <remarks>If this is being utilzie by Hangfire, ensure deleteIssuerId is supplied.  Typically if called by Hangfire it would be initiated via API, therefore, pull user id
        /// metadata before hangfire job is queued. </remarks>
        void DeleteJob(int id, string deleteIssuerId = null, bool logicalDelete = true);

        /// <summary>
        /// Find retriever job associated with dataflow and set objectstatus
        /// based on logicalDeleteflag: true = "Pending Delete", false = "Deleted"
        /// If deleteIssuerId is not specified, userService will be used to pull current user Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deleteIssuerId">If not supplied, will utilize userService to pull current user</param>
        /// <param name="logicalDelete">Perform soft or hard delete</param>
        void DeleteJobByDataFlowId(int id, string deleteIssuerId = null, bool logicalDelete = true);

        List<RetrieverJob> GetDfsRetrieverJobs();
    }
}

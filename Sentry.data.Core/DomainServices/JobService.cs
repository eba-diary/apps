using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Sentry.data.Core.RetrieverJobOptions;
using Hangfire;
using System.Reflection;

namespace Sentry.data.Core
{
    public class JobService : IJobService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;

        public JobService(IDatasetContext datasetContext, IUserService userService)
        {
            _datasetContext = datasetContext;
            _userService = userService;
        }

        public JobHistory GetLastExecution(RetrieverJob job)
        {
            return _datasetContext.JobHistory.Where(w => w.JobId.Id == job.Id && w.State == GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE).OrderByDescending(o => o.Created).Take(1).SingleOrDefault();
        }

        public List<Submission> GetJobSubmissions(int jobId, int submissionId = 0)
        {
            List<Submission> subList;

            subList = (submissionId != 0)? _datasetContext.Submission.Where(w => w.JobId.Id == jobId && w.SubmissionId == submissionId).ToList() : 
                _datasetContext.Submission.Where(w => w.JobId.Id == jobId).ToList();

            return subList;
        }

        public List<JobHistory> GetJobHistoryBySubmission(int SubmissionId)
        {
            List<JobHistory> jobHistoryList = _datasetContext.JobHistory.Where(w => w.Submission.SubmissionId == SubmissionId).ToList();
            return jobHistoryList;
        }

        public List<JobHistory> GetJobHistoryByJobAndSubmission(int JobId, int SubmissionId)
        {
            List<JobHistory> histRecordList = _datasetContext.JobHistory.Where(w => w.JobId.Id == JobId && w.Submission.SubmissionId == SubmissionId).ToList();
            return histRecordList;
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
                    Active = false
                };

                //only started state gets active set to true, all others it is set to false.
                if (state == GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE) { histRecord.Active = true; };
            }            

            //if job has completed (state is success or failed), set the original active indicator to false.
            if (state == GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE || state == GlobalConstants.JobStates.RETRIEVERJOB_FAILED_STATE)
            {
                JobHistory orignialRecord = _datasetContext.JobHistory.Where(w => w.JobId.Id == job.Id && w.JobGuid == submission.JobGuid && w.Active == true).SingleOrDefault();
                if (orignialRecord != null) { orignialRecord.Active = false; };                
            };

            _datasetContext.Add(histRecord);
            _datasetContext.SaveChanges();
            
        }

        public Submission SaveSubmission(RetrieverJob job, string options)
        {
            Submission sub = job.ToSubmission();

            _datasetContext.Add(sub);
            _datasetContext.SaveChanges();

            return sub;
        }

        public RetrieverJob FindBasicJob(RetrieverJob job)
        {
            RetrieverJob basicJob = null;

            basicJob = _datasetContext.RetrieverJob.Where(w => w.DatasetConfig.ConfigId == job.DatasetConfig.ConfigId && w.DataSource is S3Basic).FetchAllConfiguration(_datasetContext).SingleOrDefault();

            if (basicJob == null)
            {
                job.JobLoggerMessage("Info", "No S3Basic job found for Schema... Finding DfsBasic job");
                basicJob = _datasetContext.RetrieverJob.Where(w => w.DatasetConfig.ConfigId == job.DatasetConfig.ConfigId && w.DataSource is DfsBasic).FetchAllConfiguration(_datasetContext).SingleOrDefault();

                if (basicJob == null)
                {
                    job.JobLoggerMessage("Fatal", "Failed to find basic job");
                    throw new NotImplementedException("Failed to find generic Basic job");
                }
            }

            return basicJob;
        }

        public RetrieverJob InstantiateJobsForCreation(DatasetFileConfig dfc, DataSource dataSource)
        {
            RetrieverJob newJob = InstantiateJob(dfc, dataSource, null);
            //_datasetContext.Add(newJob);
            return newJob;
        }

        public RetrieverJob InstantiateJobsForCreation(DataFlow df, DataSource dataSource)
        {
            RetrieverJob newJob = InstantiateJob(null, dataSource, df);
            //_datasetContext.Add(newJob);
            return newJob;
        }


        public RetrieverJob CreateAndSaveRetrieverJob(RetrieverJobDto dto)
        {
            Compression compress = new Compression();
            if (dto.IsCompressed)
            {
                MapToCompression(dto, compress);
            }

            HttpsOptions ho = new HttpsOptions();
            MaptToHttpsOptions(dto, ho);

            RetrieverJobOptions rjo = new RetrieverJobOptions();
            MapToRetrieverJobOptions(dto, rjo);
            rjo.CompressionOptions = compress;
            rjo.HttpOptions = ho;

            RetrieverJob job = new RetrieverJob();
            MapToRetrieverJob(dto, job);
            job.JobOptions = rjo;
            job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Active;
            job.DeleteIssueDTM = DateTime.MaxValue;

            _datasetContext.Add(job);

            CreateDropLocation(job);

            return job;
        }

        public void CreateDropLocation(List<RetrieverJob> jobList)
        {
            foreach (RetrieverJob job in jobList)
            {
                CreateDropLocation(job);
            }
        }

        public void CreateDropLocation(RetrieverJob job)
        {
            try
            {
                if ((job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsDataFlowBasic>())  && !System.IO.Directory.Exists(job.GetUri().LocalPath))
                {
                    System.IO.Directory.CreateDirectory(job.GetUri().LocalPath);
                }
            }
            catch (Exception e)
            {

                StringBuilder errmsg = new StringBuilder();
                errmsg.AppendLine("Failed to Create Drop Location:");
                errmsg.AppendLine($"DatasetId: {job.DatasetConfig.ParentDataset.DatasetId}");
                errmsg.AppendLine($"DatasetName: {job.DatasetConfig.ParentDataset.DatasetName}");
                errmsg.AppendLine($"DropLocation: {job.GetUri().LocalPath}");

                Logger.Error(errmsg.ToString(), e);
            }
        }

        public void DisableJob(int id)
        {
            string methodName = MethodBase.GetCurrentMethod().Name.ToLower();
            Logger.Debug($"Start method <{methodName}>");
            try
            {
                RetrieverJob job = _datasetContext.GetById<RetrieverJob>(id);

                if (job != null)
                {
                    Logger.Debug($"disabling job - jobid:{job.Id}:::datasource:{job.DataSource.Name}");

                    job.IsEnabled = false;
                    job.Modified = DateTime.Now;
                    job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Disabled;

                    _datasetContext.SaveChanges();
                }
                else
                {
                    Logger.Debug($"job not found - jobid:{id.ToString()}");
                }
                
            } catch (Exception ex)
            {
                Logger.Error($"{methodName} - failed to disable job - jobid:{id}", ex);
                throw;
            }
            Logger.Debug($"End method <{methodName}>");
        }

        public void EnableJob(int id)
        {
            string methodName = MethodBase.GetCurrentMethod().Name.ToLower();
            Logger.Debug($"Start method <{methodName}>");
            try
            {
                RetrieverJob job = _datasetContext.GetById<RetrieverJob>(id);

                if (job == null)
                {
                    Logger.Debug($"job not found - jobid:{id}");
                    return;
                }

                if (job.ObjectStatus != GlobalEnums.ObjectStatusEnum.Disabled)
                {
                    Logger.Debug($"{methodName} - job has status of DELETED, will not be enabled - jobid:{id}");
                    throw new InvalidOperationException("Cannot enable DELETED job");
                }

                Logger.Debug($"enabling job - jobid:{id}:::datasource:{job.DataSource.Name}");

                job.IsEnabled = true;
                job.Modified = DateTime.Now;
                job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Active;

                _datasetContext.SaveChanges();

            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} - failed to enable job - jobid:{id}", ex);
                throw;
            }

            Logger.Debug($"End method <{methodName}>");
        }

        /// <summary>
        /// Find retriever job associated with dataflow and set objectstatus
        /// based on logicalDeleteflag: true = "Pending Delete", false = "Deleted"
        /// If deleteIssuerId is not specified, userService will be used to pull current user Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deleteIssuerId">If not supplied, will utilize userService to pull current user</param>
        /// <param name="logicalDelete">Perform soft or hard delete</param>
        public void DeleteJobByDataFlowId(int id, string deleteIssuerId = null, bool logicalDelete = true)
        {
            //Find associated retriever job
            List<int> jobList = _datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == id).Select(s => s.Id).ToList();

            //Mark retriever jobs deleted
            Logger.Info($"{nameof(JobService)}-{nameof(DeleteJobByDataFlowId)}-deleteretrieverjob - dataflowid:{id}");
            foreach (int job in jobList)
            {
                DeleteJob(job, deleteIssuerId:deleteIssuerId, logicalDelete:logicalDelete);
            }
        }
        /// <summary>
        /// Will perform Delete fore each job id provided.
        /// If deleteIssuerId is not specified, userService will be used to pull current user Id.
        /// </summary>
        /// <param name="idList">List of RetrieverJob identifiers</param>
        /// <param name="deleteIssuerId">If not supplied, user Id will be pulled from User service</param>
        /// <param name="logicalDelete">Perform soft or hard delete</param>
        public void DeleteJob(List<int> idList, string deleteIssuerId = null, bool logicalDelete = true)
        {
            foreach (int jobId in idList)
            {
                DeleteJob(jobId, deleteIssuerId:deleteIssuerId, logicalDelete:logicalDelete);
            }
        }
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
        public void DeleteJob(int id, string deleteIssuerId = null, bool logicalDelete = true)
        {
            try
            {
                //Get RetrieverJob
                RetrieverJob job = _datasetContext.GetById<RetrieverJob>(id);

                if (logicalDelete)
                {
                    //Remove job from HangFire scheduler
                    DeleteJobFromScheduler(job);

                    //Mark job for deletion
                    job.IsEnabled = false;
                    job.Modified = DateTime.Now;
                    job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
                    job.DeleteIssueDTM = DateTime.Now;
                    job.DeleteIssuer = deleteIssuerId?? _userService.GetCurrentUser().AssociateId;
                }
                else
                {
                    //Remove job from HangFire scheduler
                    DeleteJobFromScheduler(job);

                    //Remove any associated DFS drop location
                    DeleteDFSDropLocation(job);

                    //Mark job as deleted
                    job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;
                    job.IsEnabled = false;


                    /************************************************
                     * During conversion for CLA3332, performing dataflow deletes will not do 
                     *  through Logical delete first, therefore, some of the 
                     *  values (DeleteIssuer, DeleteIssueDTM) need to be set within this else block. 
                    ************************************************/
                    if (string.IsNullOrEmpty(job.DeleteIssuer))
                    {
                        job.DeleteIssuer =  !string.IsNullOrEmpty(deleteIssuerId) ? deleteIssuerId: _userService.GetCurrentUser().AssociateId;
                    }
                    
                    //Only comparing date since the milliseconds percision are different, therefore, never evaluates true
                    //  https://stackoverflow.com/a/44324883
                    if (DateTime.MaxValue.Date == job.DeleteIssueDTM.Date)
                    {
                        job.DeleteIssueDTM = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"jobservice-deletejob-failed - jobid:{id}", ex);
                throw;
            }
        }


        public List<RetrieverJob> GetDfsRetrieverJobs()
        {
            try
            {
                List<RetrieverJob> jobs;
                jobs = _datasetContext.RetrieverJob.WhereActive().FetchAllConfiguration(_datasetContext).ToList();

                return jobs.Where(x => x.DataSource.Is<DfsDataFlowBasic>()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("<jobservice-getdfsretrieverjobs> Failed retrieving job list", ex);
                throw;
            }
        }


        #region Private Methods

        private RetrieverJob InstantiateJob(DatasetFileConfig dfc, DataSource dataSource, DataFlow df)
        {
            Guid g = Guid.NewGuid();
            RetrieverJobOptions.Compression compression = new RetrieverJobOptions.Compression()
            {
                IsCompressed = false,
                CompressionType = null,
                FileNameExclusionList = new List<string>()
            };

            RetrieverJobOptions rjo = new RetrieverJobOptions()
            {
                OverwriteDataFile = false,
                TargetFileName = "",
                CreateCurrentFile = false,
                IsRegexSearch = true,
                SearchCriteria = "\\.",
                CompressionOptions = compression
            };

            RetrieverJob rj = new RetrieverJob()
            {
                TimeZone = "Central Standard Time",
                RelativeUri = null,
                DataSource = dataSource,
                DatasetConfig = dfc,
                FileSchema = null,
                DataFlow = df,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsGeneric = true,
                JobOptions = rjo,
                JobGuid = g,
                ObjectStatus = GlobalEnums.ObjectStatusEnum.Active,
                DeleteIssueDTM = DateTime.MaxValue
            };

            if (dataSource.Is<S3Basic>())
            {
                rj.Schedule = "*/1 * * * *";
            }
            else if (dataSource.Is<DfsBasic>() || dataSource.Is<DfsDataFlowBasic>())
            {
                rj.Schedule = "Instant";
            }
            else
            {
                throw new NotImplementedException("This method does not support this type of Data Source");
            }

            return rj;
        }

        private void MapToCompression(RetrieverJobDto dto, Compression compress)
        {
            compress.IsCompressed = dto.IsCompressed;
            compress.CompressionType = dto.CompressionType.ToString();
            compress.FileNameExclusionList = dto.FileNameExclusionList ?? new List<string>();
        }

        private void MapToRetrieverJobOptions(RetrieverJobDto dto, RetrieverJobOptions jobOptions)
        {
            jobOptions.IsRegexSearch = true;
            jobOptions.OverwriteDataFile = false;
            jobOptions.SearchCriteria = dto.SearchCriteria;
            jobOptions.CreateCurrentFile = dto.CreateCurrentFile;
            jobOptions.FtpPattern = dto.FtpPattern?? FtpPattern.NoPattern;
            jobOptions.TargetFileName = dto.TargetFileName;
        }

        private void MaptToHttpsOptions(RetrieverJobDto dto, HttpsOptions httpOptions)
        {
            httpOptions.Body = dto.HttpRequestBody;
            httpOptions.RequestMethod = dto.RequestMethod?? HttpMethods.none;
            httpOptions.RequestDataFormat = dto.RequestDataFormat?? HttpDataFormat.none;
        }

        private void MapToRetrieverJob(RetrieverJobDto dto, RetrieverJob job)
        {
            job.Created = (job.Id == 0) ? DateTime.Now : job.Created;
            job.DatasetConfig = (dto.DatasetFileConfig == 0) ? null : _datasetContext.GetById<DatasetFileConfig>(dto.DatasetFileConfig);
            job.DataSource = _datasetContext.GetById<DataSource>(dto.DataSourceId);
            job.FileSchema = (dto.FileSchema == 0) ? null : _datasetContext.GetById<FileSchema>(dto.FileSchema);
            job.DataFlow = (dto.DataFlow == 0) ? null : _datasetContext.GetById<DataFlow>(dto.DataFlow);
            job.IsGeneric = false;
            job.JobGuid = (job.JobGuid == Guid.Empty) ? Guid.NewGuid() : job.JobGuid;
            job.Modified = DateTime.Now;
            job.RelativeUri = dto.RelativeUri;
            job.Schedule = dto.Schedule;
            job.TimeZone = "Central Standard Time";
            job.ObjectStatus = dto.ObjectStatus;
            job.DeleteIssueDTM = dto.DeleteIssueDTM;
            job.DeleteIssuer = dto.DeleteIssuer;
        }

        private void DeleteJobFromScheduler(RetrieverJob job)
        {
            //Remove jobs from Hangfire if they exist
            RecurringJob.RemoveIfExists(job.JobName());
        }

        private void DeleteDFSDropLocation(RetrieverJob job)
        {
            //For DFS type jobs, remove drop folder from network location
            if (job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsBasicHsz>() || job.DataSource.Is<DfsDataFlowBasic>())
            {
                string dfsPath = job.GetUri().LocalPath;

                if (System.IO.Directory.Exists(dfsPath))
                {
                    Logger.Debug($"jobservice-deletedfsdropLocation - dfs-directory-detected - JobId:{job.Id}");
                    List<string> files = System.IO.Directory.EnumerateFiles(dfsPath).ToList();
                    if (files.Any())
                    {
                        Logger.Debug($"jobservice-deletedfsdropLocation dfs-directory-file-detected - deleteing {files.Count} file(s) from {dfsPath} - JobId:{job.Id}");
                        foreach (string file in files)
                        {
                            System.IO.File.Delete(file);
                        }
                    }

                    Logger.Debug($"jobservice-deletedfsdropLocation dfs-directory-delete path:{dfsPath} - JobId:{job.Id}");
                    System.IO.Directory.Delete(dfsPath);
                }
                else
                {
                    Logger.Debug($"jobservice-deletedfsdropLocation - dfs-directory-not-detected - JobId:{job.Id}");
                }
            }
        }
        #endregion
    }
}

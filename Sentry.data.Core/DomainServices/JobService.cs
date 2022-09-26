using Hangfire;
using Nest;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Core
{
    public class JobService : IJobService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly IRecurringJobManager _recurringJobManager;

        public JobService(IDatasetContext datasetContext, IUserService userService,
            IRecurringJobManager recurringJobManager)
        {
            _datasetContext = datasetContext;
            _userService = userService;
            _recurringJobManager = recurringJobManager;
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
            string methodName = $"{nameof(JobService)}_{nameof(InstantiateJobsForCreation)}";
            Logger.Info($"{methodName} Method Start");

            RetrieverJob newJob = InstantiateJob(null, dataSource, df);

            Logger.Info($"{methodName} Method End");
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
            MapToHttpsOptions(dto, ho);

            RetrieverJobOptions rjo = new RetrieverJobOptions();
            MapToRetrieverJobOptions(dto, rjo);
            rjo.CompressionOptions = compress;
            rjo.HttpOptions = ho;

            RetrieverJob job = new RetrieverJob();
            MapToRetrieverJob(dto, job);
            job.JobOptions = rjo;
            job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Active;
            job.DeleteIssueDTM = DateTime.MaxValue;

            //if we have previous execution parameters from a data flow edit, only keep if RelativeUri did not change
            if (dto.ExecutionParameters?.Any() == true && dto.FileSchema > 0)
            {
                //check that all jobs are currently deleted and get the most recent deleted job
                RetrieverJob previousRetrieverJob = _datasetContext.RetrieverJob.OrderByDescending(x => x.Id).FirstOrDefault(w => w.DataFlow.SchemaId == dto.FileSchema);
                if (previousRetrieverJob != null && previousRetrieverJob.ObjectStatus == GlobalEnums.ObjectStatusEnum.Deleted && previousRetrieverJob.RelativeUri == job.RelativeUri)
                {
                    job.ExecutionParameters = dto.ExecutionParameters;
                }
            }
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
            string methodName = $"{nameof(DataFlowService).ToLower()}_{nameof(CreateDropLocation).ToLower()}";
            Logger.Info($"{methodName} Method Start");

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
            Logger.Info($"{methodName} Method End");
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

        public bool Delete(List<int> idList, IApplicationUser user, bool logicalDelete)
        {
            bool allDeletesSuccessfull = true;
            foreach (int jobId in idList)
            {
                bool wasJobDeleteSuccessful = Delete(jobId, user, logicalDelete);
                
                //reflect any unsuccessful deletes to allDeletesSuccessfull
                if (!wasJobDeleteSuccessful)
                {
                    allDeletesSuccessfull = wasJobDeleteSuccessful;
                }
            }
            return allDeletesSuccessfull;
        }
        /// <summary>
        /// Mark retriever job objectstatus based
        /// on logicalDelete flag: true = "Pending Delete", false = "Deleted".
        /// If deleteIssuerId is not specified, userService will be used to pull current user Id.
        /// </summary>
        /// <param name="id">RetrieverJob identifier</param>
        /// <param name="user">If not supplied, will utilize userService to pull current user</param>
        /// <param name="logicalDelete">Perform soft or hard delete</param>
        /// <remarks>If this is being utilzie by Hangfire, ensure deleteIssuerId is supplied.  Typically if called by Hangfire it would be initiated via API, therefore, pull user id
        /// metadata before hangfire job is queued. </remarks>
        public bool Delete(int id, IApplicationUser user, bool logicalDelete)
        {
            string methodName = $"{nameof(JobService).ToLower()}_{nameof(Delete).ToLower()}";
            Logger.Debug($"{methodName} Start Method");

            bool returnResult = true;
            bool isAlreadyDeleted = false;
            try
            {
                //Get RetrieverJob
                RetrieverJob job = _datasetContext.GetById<RetrieverJob>(id);

                //return false if job id not found
                if (job == null)
                {
                    Logger.Warn($"{methodName} job not found (id:{id}");
                    return false;
                }

                //If retriever job is already deleted, stop processing
                if ((!logicalDelete && job.ObjectStatus == GlobalEnums.ObjectStatusEnum.Deleted) ||
                    (logicalDelete && job.ObjectStatus == GlobalEnums.ObjectStatusEnum.Pending_Delete))
                {
                    isAlreadyDeleted = true;
                }


                if (isAlreadyDeleted)
                {
                    Logger.Debug($"{methodName} retiever job already deleted (jobid:{job.Id})");
                }
                else if (logicalDelete)
                {
                    //Remove job from HangFire scheduler
                    DeleteJobFromScheduler(job);

                    //Mark job for deletion
                    job.IsEnabled = false;
                    job.Modified = DateTime.Now;
                    job.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
                    job.DeleteIssueDTM = DateTime.Now;
                    job.DeleteIssuer = (user == null)? _userService.GetCurrentUser().AssociateId : user.AssociateId;
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
                     * During conversion for CLA3332, performing dataflow deletes will not go 
                     *  through Logical delete first, therefore, some of the 
                     *  values (DeleteIssuer, DeleteIssueDTM) need to be set within this else block. 
                    ************************************************/
                    if (string.IsNullOrEmpty(job.DeleteIssuer))
                    {
                        job.DeleteIssuer =  (user != null) ? user.AssociateId : _userService.GetCurrentUser().AssociateId;
                    }
                    
                    //Only comparing date since the milliseconds percision are different, therefore, never evaluates true
                    //  https://stackoverflow.com/a/44324883
                    if (DateTime.MaxValue.Date == job.DeleteIssueDTM.Date)
                    {
                        job.DeleteIssueDTM = DateTime.Now;
                    }
                }

                Logger.Debug($"{methodName} End Method");
                return returnResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} - failed - jobid:{id}", ex);
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
            string methodName = $"{nameof(JobService)}_{nameof(InstantiateJob)}";
            Logger.Info($"{methodName} Method Start");

            Guid g = Guid.NewGuid();

            Logger.Debug($"{methodName} compression object creation start");
            RetrieverJobOptions.Compression compression = new RetrieverJobOptions.Compression()
            {
                IsCompressed = false,
                CompressionType = null,
                FileNameExclusionList = new List<string>()
            };
            Logger.Debug($"{methodName} compression object creation end");

            Logger.Debug($"{methodName} retrieverjoboptions object creation start");
            RetrieverJobOptions rjo = new RetrieverJobOptions()
            {
                OverwriteDataFile = false,
                TargetFileName = "",
                CreateCurrentFile = false,
                IsRegexSearch = true,
                SearchCriteria = "\\.",
                CompressionOptions = compression
            };
            Logger.Debug($"{methodName} retrieverjoboptions object creation end");

            Logger.Debug($"{methodName} retrieverjob object creation start");
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
            Logger.Debug($"{methodName} retrieverjob object creation End");

            Logger.Debug($"{methodName} retrieverjob schedule creation start");
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
            Logger.Debug($"{methodName} retrieverjob schedule creation end");

            Logger.Info($"{methodName} Method End");

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

        private void MapToHttpsOptions(RetrieverJobDto dto, HttpsOptions httpOptions)
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
            _recurringJobManager.RemoveIfExists(job.JobName());
        }

        private void DeleteDFSDropLocation(RetrieverJob job)
        {
            Logger.Debug($"{nameof(JobService).ToLower()}_{nameof(DeleteDFSDropLocation).ToLower()} Method Start");
            //For DFS type jobs, remove drop folder from network location
            if (job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsBasicHsz>() || job.DataSource.Is<DfsDataFlowBasic>())
            {
                string dfsPath = job.GetUri().LocalPath;

                if (System.IO.Directory.Exists(dfsPath))
                {
                    Logger.Debug($"{nameof(JobService).ToLower()}_{nameof(DeleteDFSDropLocation).ToLower()} - JobId:{job.Id}");
                    List<string> files = System.IO.Directory.EnumerateFiles(dfsPath).ToList();
                    if (files.Any())
                    {
                        Logger.Debug($"{nameof(JobService).ToLower()}_{nameof(DeleteDFSDropLocation).ToLower()} dfs-directory-file-detected - deleteing {files.Count} file(s) from {dfsPath} - JobId:{job.Id}");
                        foreach (string file in files)
                        {
                            System.IO.File.Delete(file);
                        }
                    }

                    Logger.Debug($"{nameof(JobService).ToLower()}_{nameof(DeleteDFSDropLocation).ToLower()} dfs-directory-delete path:{dfsPath} - JobId:{job.Id}");
                    System.IO.Directory.Delete(dfsPath);
                }
                else
                {
                    Logger.Debug($"{nameof(JobService).ToLower()}_{nameof(DeleteDFSDropLocation).ToLower()} - dfs-directory-not-detected - JobId:{job.Id}");
                }
            }

            Logger.Debug($"{nameof(JobService).ToLower()}_{nameof(DeleteDFSDropLocation).ToLower()} Method End");
        }
        #endregion
    }
}

using Hangfire;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Job;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.Livy;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces.QuartermasterRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.RetrieverJobOptions;

namespace Sentry.data.Core
{
    public class JobService : IJobService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IDataFeatures _dataFeatures;
        private readonly IApacheLivyProvider _apacheLivyProvider;
        private readonly IDfsRetrieverJobProvider _environmentRetrieverJobProvider;

        public JobService(IDatasetContext datasetContext, IUserService userService,
            IRecurringJobManager recurringJobManager, IDataFeatures dataFeatures,
            IApacheLivyProvider apacheLivyProvider, IDfsRetrieverJobProvider environmentRetrieverJobProvider)
        {
            _datasetContext = datasetContext;
            _userService = userService;
            _recurringJobManager = recurringJobManager;
            _dataFeatures = dataFeatures;
            _apacheLivyProvider = apacheLivyProvider;
            _environmentRetrieverJobProvider = environmentRetrieverJobProvider;
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

        /// <summary>
        /// Returns the dfs datasource based on the NamedEnvironmentType
        /// </summary>
        /// <param name="namedEnviornmentType"></param>
        /// <remarks>This is used by <see cref="DataApplicationService.CreateWithoutSave_DfsRetrieverJob"/>.</remarks>
        /// <returns></returns>
        public DataSource GetDfsDataSourceByDatasetNamedEnvironmentType(NamedEnvironmentType namedEnviornmentType)
        {
            DataSource dfsDataSource;
            if (string.IsNullOrEmpty(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
            {
                if (namedEnviornmentType == NamedEnvironmentType.NonProd)
                {
                    dfsDataSource = _datasetContext.DataSources.FirstOrDefault(x => x is DfsNonProdSource);
                }
                else
                {
                    dfsDataSource = _datasetContext.DataSources.FirstOrDefault(x => x is DfsProdSource);
                }
            }
            else
            {
                dfsDataSource = _datasetContext.DataSources.FirstOrDefault(w => w is DfsDataFlowBasic);
            }
            return dfsDataSource;
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

        public void CreateDfsRetrieverJob(DataFlow dataFlow)
        {
            NamedEnvironmentType datasetEnvironmentType = _datasetContext.Datasets.Where(x => x.DatasetId == dataFlow.DatasetId).Select(x => x.NamedEnvironmentType).FirstOrDefault();
            DataSource dfsDataSource = GetDfsDataSourceByDatasetNamedEnvironmentType(datasetEnvironmentType);

            _ = CreateDfsRetrieverJob(dataFlow, dfsDataSource);
        }

        public RetrieverJob CreateDfsRetrieverJob(DataFlow df, DataSource dataSource)
        {
            Logger.Info($"InstantiateJobsForCreation Method Start");

            RetrieverJob rj = new RetrieverJob()
            {
                TimeZone = "Central Standard Time",
                RelativeUri = null,
                DataSource = dataSource,
                DatasetConfig = null,
                FileSchema = null,
                DataFlow = df,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsGeneric = true,
                JobOptions = new RetrieverJobOptions()
                {
                    OverwriteDataFile = false,
                    TargetFileName = "",
                    CreateCurrentFile = false,
                    IsRegexSearch = true,
                    SearchCriteria = "\\.",
                    CompressionOptions = new Compression()
                    {
                        IsCompressed = false,
                        CompressionType = null,
                        FileNameExclusionList = new List<string>()
                    }
                },
                JobGuid = Guid.NewGuid(),
                ObjectStatus = ObjectStatusEnum.Active,
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
            else if (dataSource.Is<DfsEnvironmentSource>())
            {
                rj.Schedule = "Instant";
                rj.RelativeUri = $"{df.SaidKeyCode.ToUpper()}/{df.NamedEnvironment.ToUpper()}/{df.FlowStorageCode}";
            }
            else
            {
                throw new NotImplementedException("This method does not support this type of Data Source");
            }

            _datasetContext.Add(rj);

            Logger.Info($"InstantiateJobsForCreation Method End");

            return rj;
        }

        public RetrieverJob CreateRetrieverJob(RetrieverJobDto dto)
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
            job.ObjectStatus = ObjectStatusEnum.Active;
            job.DeleteIssueDTM = DateTime.MaxValue;

            //if we have previous execution parameters from a data flow edit, only keep if RelativeUri did not change
            if (dto.ExecutionParameters?.Any() == true && dto.FileSchema > 0)
            {
                //check that all jobs are currently deleted and get the most recent deleted job
                RetrieverJob previousRetrieverJob = _datasetContext.RetrieverJob.OrderByDescending(x => x.Id).FirstOrDefault(w => w.DataFlow.SchemaId == dto.FileSchema);
                if (previousRetrieverJob != null && previousRetrieverJob.ObjectStatus == ObjectStatusEnum.Deleted && previousRetrieverJob.RelativeUri == job.RelativeUri)
                {
                    job.ExecutionParameters = dto.ExecutionParameters;
                }
            }
            _datasetContext.Add(job);

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
            Logger.Info($"CreateDropLocation Method Start");

            try
            {
                if (job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsDataFlowBasic>() || job.DataSource.Is<DfsEnvironmentSource>())
                {
                    string localPath = job.GetUri().LocalPath;
                    if (!System.IO.Directory.Exists(localPath))
                    {
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                }
            }
            catch (Exception e)
            {
                StringBuilder errmsg = new StringBuilder();
                errmsg.AppendLine("Failed to Create Drop Location:");
                errmsg.AppendLine($"DropLocation: {job.GetUri().LocalPath}");

                Logger.Error(errmsg.ToString(), e);
            }

            Logger.Info($"CreateDropLocation Method End");
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

        public List<DfsMonitorDto> GetDfsRetrieverJobs(string requestingNamedEnvironment)
        {
            List<RetrieverJob> jobs;

            if (string.IsNullOrEmpty(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
            {
                if (string.IsNullOrEmpty(requestingNamedEnvironment) || _environmentRetrieverJobProvider.AcceptedNamedEnvironments.Contains(requestingNamedEnvironment.ToUpper()))
                {
                    jobs = _environmentRetrieverJobProvider.GetDfsRetrieverJobs(requestingNamedEnvironment);
                }
                else
                {
                    string validEnvs = string.Join(", ", _environmentRetrieverJobProvider.AcceptedNamedEnvironments);
                    throw new DfsRetrieverJobException($"The requesting named environment '{requestingNamedEnvironment}' is not an accepted named environment ({validEnvs}).");
                }
            }
            else
            {
                jobs = _datasetContext.RetrieverJob.Where(x => x.ObjectStatus == ObjectStatusEnum.Active && x.DataSource is DfsDataFlowBasic)
                    .Fetch(x => x.DataSource)
                    .Fetch(x => x.DataFlow)
                    .ToList();
            }

            List<DfsMonitorDto> dtos = new List<DfsMonitorDto>();
            foreach (var job in jobs)
            {
                Uri dataSourceUri = job.GetUri();
                DfsMonitorDto dto = new DfsMonitorDto()
                {
                    JobId = job.Id,
                    MonitorTarget = dataSourceUri.LocalPath
                };

                dtos.Add(dto);
            }

            return dtos;
        }

        public Task<System.Net.Http.HttpResponseMessage> SubmitApacheLivyJobAsync(int JobId, Guid JobGuid, JavaOptionsOverrideDto dto)
        {
            if (JobId == 0)
            {
                throw new ArgumentNullException(nameof(JobId), "JobId required");
            }

            if (JobGuid == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(JobGuid), "JobGuid required");
            }

            RetrieverJob job = _datasetContext.RetrieverJob.FirstOrDefault(w => w.Id == JobId && JobGuid == w.JobGuid);

            if (job == null)
            {
                throw new JobNotFoundException($"JobId:{JobId} | JobGuid:{JobGuid}");
            }

            if (!job.DataSource.Is<JavaAppSource>())
            {
                throw new ArgumentOutOfRangeException(nameof(JobId), "This only submits job defined with a data source type of JavaApp");
            }



            return SubmitApacheLivyJobInternalAsync(job, JobGuid, dto);

        }

        public Task<System.Net.Http.HttpResponseMessage> GetApacheLivyBatchStatusAsync(JobHistory historyRecord)
        {
            Logger.Debug($"{nameof(GetApacheLivyBatchStatusAsync)} - Method Start");
            if (historyRecord == null)
            {
                throw new ArgumentNullException(nameof(historyRecord), "History Record Required");
            }

            Logger.Debug($"{nameof(GetApacheLivyBatchStatusAsync)} - Method End");
            return GetApacheLivyBatchStatusInternalAsync(historyRecord);
        }

        public Task<System.Net.Http.HttpResponseMessage> GetApacheLivyBatchStatusAsync(int jobId, int batchId)
        {
            if (jobId == 0)
            {
                throw new ArgumentNullException(nameof(jobId), "Job Id Required");
            }
            if (batchId == 0)
            {
                throw new ArgumentNullException(nameof(batchId), "Batch Id Required");
            }

            JobHistory hr = _datasetContext.JobHistory.FirstOrDefault(w => w.JobId.Id == jobId && w.BatchId == batchId && w.Active);

            if (hr == null)
            {
                Logger.Info($"{nameof(GetApacheLivyBatchStatusAsync)} - No history record returned.");
                return Task.FromResult(default(System.Net.Http.HttpResponseMessage));
            }
                
            return GetApacheLivyBatchStatusInternalAsync(hr);
        }

        #region Private Methods

        private async Task<System.Net.Http.HttpResponseMessage> GetApacheLivyBatchStatusInternalAsync(JobHistory historyRecord)
        {
            Logger.Debug($"{nameof(GetApacheLivyBatchStatusInternalAsync)} - Method Start");
            /*
            * Add flowexecutionguid context variable
            */
            string flowExecutionGuid = (historyRecord.Submission != null && historyRecord.Submission.FlowExecutionGuid != null) ? historyRecord.Submission.FlowExecutionGuid : "00000000000000000";
            Logger.AddContextVariable(new TextVariable("flowexecutionguid", flowExecutionGuid));

            /*
             * Add runinstanceguid context variable
             */
            string runInstanceGuid = (historyRecord.Submission != null && historyRecord.Submission.RunInstanceGuid != null) ? historyRecord.Submission.RunInstanceGuid : "00000000000000000";
            Logger.AddContextVariable(new TextVariable("runinstanceguid", runInstanceGuid));

            string clusterUrl = GetClusterUrl(historyRecord);

            Logger.Info($"{nameof(GetApacheLivyBatchStatusInternalAsync).ToLower()} - pull batch metadata: batchId:{historyRecord.BatchId} apacheLivyUrl:{clusterUrl}/batches/{historyRecord.BatchId}");

            _apacheLivyProvider.SetBaseUrl(clusterUrl);

            System.Net.Http.HttpResponseMessage response = await _apacheLivyProvider.GetRequestAsync($"/batches/{historyRecord.BatchId}").ConfigureAwait(false);

            string result = await response.Content.ReadAsStringAsync();
            string sendresult = (string.IsNullOrEmpty(result)) ? "noresultsupplied" : result;

            Logger.Info($"{nameof(GetApacheLivyBatchStatusInternalAsync).ToLower()} - getbatchstate_livyresponse batchId:{historyRecord.BatchId} statuscode:{response.StatusCode}:::result:{sendresult}");

            LivyReply lr = null;

            if (response.IsSuccessStatusCode)
            {
                if (result == $"Session '{historyRecord.BatchId}' not found.")
                {
                    Logger.Info($"{nameof(GetApacheLivyBatchStatusInternalAsync)} - Session not found");
                }
                else
                {
                    lr = JsonConvert.DeserializeObject<LivyReply>(result);
                }
            }

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                JobHistory newHistoryRecord = MapToJobHistory(historyRecord, lr);

                if (string.IsNullOrEmpty(newHistoryRecord.ClusterUrl)) { newHistoryRecord.ClusterUrl = clusterUrl; }

                _datasetContext.Add(newHistoryRecord);

                //set previous active record to inactive
                historyRecord.Modified = DateTime.Now;
                historyRecord.Active = false;

                _datasetContext.SaveChanges();
            }

            Logger.Debug($"{nameof(GetApacheLivyBatchStatusInternalAsync)} - Method End");
            return response;
        }

        internal JobHistory MapToJobHistory(JobHistory previousHistoryRec, LivyReply reply)
        {
            //create history record and set it active
            return new JobHistory()
            {
                JobId = previousHistoryRec.JobId,
                BatchId = previousHistoryRec.BatchId,
                Created = previousHistoryRec.Created,
                Modified = DateTime.Now,
                State = (reply != null) ? reply.state : "Unknown",
                LivyAppId = (reply != null) ? reply.appId : previousHistoryRec.LivyAppId,
                LivyDriverLogUrl = (reply != null) ? reply.appInfo.Where(w => w.Key == "driverLogUrl").Select(s => s.Value).FirstOrDefault() : previousHistoryRec.LivyDriverLogUrl,
                LivySparkUiUrl = (reply != null) ? reply.appInfo.Where(w => w.Key == "sparkUiUrl").Select(s => s.Value).FirstOrDefault() : previousHistoryRec.LivySparkUiUrl,
                LogInfo = (reply?.log != null) ? string.Join("",reply.log) : "Livy did not return a status for this batch job.",
                Active = (reply != null) ? reply.IsActive() : false,
                JobGuid = previousHistoryRec.JobGuid,
                Submission = previousHistoryRec.Submission,
                ClusterUrl = previousHistoryRec.ClusterUrl
            };
        }

        internal async Task<System.Net.Http.HttpResponseMessage> SubmitApacheLivyJobInternalAsync(RetrieverJob job, Guid JobGuid, JavaOptionsOverrideDto dto)
        {
            string postContent = BuildLivyPostContent(dto, job);

            string clusterUrl = GetClusterUrl(dto);

            _apacheLivyProvider.SetBaseUrl(clusterUrl);

            System.Net.Http.HttpResponseMessage response = await _apacheLivyProvider.PostRequestAsync("batches", postContent);

            string result = await response.Content.ReadAsStringAsync();
            string postResult = (string.IsNullOrEmpty(result)) ? "noresultsupplied" : result;

            Logger.Debug($"postbatches_livyresponse statuscode:{response.StatusCode}:::result:{postResult}");

            //Record submission regardless if target deems it a bad request.
            Submission sub = MapToSubmission(job, dto);
            sub.Serialized_Job_Options = postContent;
            sub.ClusterUrl = clusterUrl;

            _datasetContext.Add(sub);
            _datasetContext.SaveChanges();

            if (response.IsSuccessStatusCode)
            {
                LivyBatch batchResult = JsonConvert.DeserializeObject<LivyBatch>(result);

                JobHistory histRecord = new JobHistory()
                {
                    JobId = job,
                    BatchId = batchResult.Id,
                    JobGuid = JobGuid,
                    State = batchResult.State,
                    LivyAppId = batchResult.Appid,
                    LivyDriverLogUrl = batchResult.AppInfo.Where(w => w.Key == "driverLogUrl").Select(s => s.Value).FirstOrDefault(),
                    LivySparkUiUrl = batchResult.AppInfo.Where(w => w.Key == "sparkUiUrl").Select(s => s.Value).FirstOrDefault(),
                    Active = true,
                    Submission = sub,
                    ClusterUrl = clusterUrl
                };

                _datasetContext.Add(histRecord);
                _datasetContext.SaveChanges();
            }

            return response;
        }

        internal virtual string GetClusterUrl(JavaOptionsOverrideDto dto)
        {
            return (!string.IsNullOrWhiteSpace(dto.ClusterUrl)) ? dto.ClusterUrl : Configuration.Config.GetHostSetting("ApacheLivy");
        }
        internal virtual string GetClusterUrl(JobHistory historyRecord)
        {
            return (!string.IsNullOrWhiteSpace(historyRecord.ClusterUrl)) ? historyRecord.ClusterUrl : Configuration.Config.GetHostSetting("ApacheLivy");
        }

        internal virtual Submission MapToSubmission(RetrieverJob job, JavaOptionsOverrideDto dto)
        {
            var submission = new Submission()
            {
                JobId = job,
                JobGuid = job.JobGuid,
                Created = DateTime.Now,
                FlowExecutionGuid = dto.FlowExecutionGuid,
                RunInstanceGuid = dto.RunInstanceGuid,
                ClusterUrl = dto.ClusterUrl
            };
            return submission;
        }

        internal virtual string BuildLivyPostContent(JavaOptionsOverrideDto dto, RetrieverJob job)
        {
            JavaAppSource dsrc = _datasetContext.GetById<JavaAppSource>(job.DataSource.Id);

            StringBuilder json = new StringBuilder();
            json.Append($"{{\"file\": \"{dsrc.Options.JarFile}\"");

            AddElement(json, "className", dsrc.Options.ClassName, null);

            if (_dataFeatures.CLA3497_UniqueLivySessionName.GetValue())
            {
                string livySessionName = GenerateUniqueLivySessionName(dsrc);
                AddElement(json, "name", livySessionName, null);
                Logger.AddContextVariable(new TextVariable("livysessionname", livySessionName));
            }
            else
            {
                json.Append($", \"name\": \"{dsrc.Name}\"");
            }

            if (dto != null)
            {

                AddElement(json, "driverMemory", dto.DriverMemory, job.JobOptions?.JavaAppOptions?.DriverMemory);

                AddElement(json, "driverCores", dto.DriverCores, job.JobOptions?.JavaAppOptions?.DriverCores);

                AddElement(json, "executorMemory", dto.ExecutorMemory, job.JobOptions?.JavaAppOptions?.ExecutorMemory);

                AddElement(json, "executorCores", dto.ExecutorCores, job.JobOptions?.JavaAppOptions?.ExecutorCores);

                AddElement(json, "numExecutors", dto.NumExecutors, job.JobOptions?.JavaAppOptions?.NumExecutors);


                AddLivyConfigElement(json, dto.ConfigurationParameters, job.JobOptions?.JavaAppOptions?.ConfigurationParameters);

                // THIS HAS BRACKETS javaOptionsOverride.ConfigurationParameters  [ ]
                AddLivyArgumentsElement(json, dto.Arguments, job.JobOptions?.JavaAppOptions?.Arguments);
            }            

            string[] jars = dsrc.Options.JarDepenencies;

            for (int i = 0; i < jars.Count(); i++)
            {
                if (i == 0)
                {
                    json.Append($", \"jars\": [");
                }
                json.Append($"\"{jars[i]}\"");
                if (i != jars.Count() - 1)
                {
                    json.Append(",");
                }
                else
                {
                    json.Append("]");
                }
            }


            //DO NOT KILL THIS
            json.Append("}");

            return json.ToString();
        }

        internal virtual string GenerateUniqueLivySessionName(JavaAppSource dsrc)
        {
            StringBuilder randomCharacterSting = new StringBuilder();

            using (var rngCryptoProvider = new RNGCryptoServiceProvider())
            {
                string validCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

                for (int i = 0; i < 6; i++)
                {
                    byte[] randomNumber = new byte[1];
                    do
                    {
                        rngCryptoProvider.GetBytes(randomNumber);
                    }
                    while (!IsWithinBounds(randomNumber[0], (byte)validCharacters.Length));
                    randomCharacterSting.Append(new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 1).Select(s => s[randomNumber[0]]).ToArray()));

                    Console.WriteLine($"{randomNumber[0]}:::{randomNumber[0] % 6}");
                }
            }

            string livySessionName = $"{dsrc.Name}_{randomCharacterSting}";
            return livySessionName;    
        }
        private static bool IsWithinBounds(byte value, byte maxValue)
        {
            return value < maxValue;
        }

        internal void AddLivyConfigElement(StringBuilder content, string newValue, string defaultValue)
        {
            string configValue = null;

            if (newValue != null && newValue.Any())
            {
                configValue = newValue;
            }
            else if (defaultValue != null && defaultValue.Any())
            {
                configValue = defaultValue;
            }

            if (configValue != null)
            {
                content.Append($", \"conf\":{configValue}");
            }
        }
        internal void AddLivyArgumentsElement(StringBuilder content, string[] newValue, string[] defaultValue)
        {
            if (newValue != null && newValue.Any())
            {
                GenerateLivyArguments(newValue, content);
            }
            else if (defaultValue != null && defaultValue.Any())
            {
                GenerateLivyArguments(defaultValue, content);
            }
        }
        internal void AddElement(StringBuilder content, string elementName, string newValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(elementName) || (String.IsNullOrEmpty(newValue) && String.IsNullOrEmpty(defaultValue)))
            {
                throw new ArgumentNullException(nameof(newValue));
            }

            if (!String.IsNullOrWhiteSpace(newValue))
            {
                content.Append($", \"{elementName}\": \"{newValue}\"");
            }
            else
            {
                content.Append($", \"{elementName}\": \"{defaultValue}\"");
            }
        }

        internal void AddElement(StringBuilder content, string elementName, int? newValue, int? defaultValue)
        {
            if (elementName == null || (newValue == null && defaultValue == null))
            {
                throw new ArgumentNullException(nameof(newValue));
            }

            if (newValue != null)
            {
                content.Append($", \"{elementName}\": {newValue}");
            }
            else
            {
                content.Append($", \"{elementName}\": {defaultValue}");
            }
        }

        private static void GenerateLivyArguments(string[] arguments, StringBuilder json)
        {
            json.Append($", \"args\": [");
            int iteration = 1;
            int argcnt = arguments.Count();
            foreach (string arg in arguments)
            {
                string argString = (iteration < argcnt) ? $"\"{arg}\"," : $"\"{arg}\"]";
                json.Append(argString);
                iteration++;
            }
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
            jobOptions.FtpPattern = dto.FtpPattern ?? FtpPattern.NoPattern;
            jobOptions.TargetFileName = dto.TargetFileName;
        }

        private void MapToHttpsOptions(RetrieverJobDto dto, HttpsOptions httpOptions)
        {
            httpOptions.Body = dto.HttpRequestBody;
            httpOptions.RequestMethod = dto.RequestMethod?? HttpMethods.none;
            httpOptions.RequestDataFormat = dto.RequestDataFormat?? HttpDataFormat.none;
            httpOptions.PagingType = dto.PagingType;
            httpOptions.PageTokenField = dto.PageTokenField;
            httpOptions.PageParameterName = dto.PageParameterName;
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
            job.RequestVariables = dto.RequestVariables?.Select(x => x.ToEntity()).ToList();
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
            if (job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsBasicHsz>() || job.DataSource.Is<DfsDataFlowBasic>() || job.DataSource.Is<DfsEnvironmentSource>())
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

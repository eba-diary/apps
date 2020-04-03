﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using static Sentry.data.Core.RetrieverJobOptions;
using Sentry.data.Core.Exceptions;

namespace Sentry.data.Core
{
    public class JobService : IJobService
    {
        private IDatasetContext _datasetContext;

        public JobService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
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

        private RetrieverJob InstantiateJob(DatasetFileConfig dfc, DataSource dataSource, DataFlow df)
        {
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
                JobOptions = rjo
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

        public RetrieverJob InstantiateJobsForCreation(DatasetFileConfig dfc, DataSource dataSource)
        {
            return InstantiateJob(dfc, dataSource, null);
        }

        public RetrieverJob InstantiateJobsForCreation(DataFlow df, DataSource dataSource)
        {
            return InstantiateJob(null, dataSource, df);
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

            _datasetContext.Add(job);

            return job;
        }

        public void CreateDropLocation(RetrieverJob job)
        {
            try
            {
                if (job.DataSource.Is<DfsBasic>()  && !System.IO.Directory.Exists(job.GetUri().LocalPath))
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
            try
            {
                RetrieverJob job = _datasetContext.GetById<RetrieverJob>(id);

                job.IsEnabled = false;
                job.Modified = DateTime.Now;

                _datasetContext.SaveChanges();
            } catch (Exception ex)
            {
                Logger.Error($"jobservice-disablejob-failed - jobid:{id}", ex);
                throw;
            }            
        }

        public void DeleteJob(int id)
        {
            try
            {
                RetrieverJob job = _datasetContext.GetById<RetrieverJob>(id);

                
            }
            catch (Exception ex)
            {
                Logger.Error($"jobservice-deletejob-failed - jobid:{id}", ex);
                throw;
            }
        }

        #region Private Methods

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
            jobOptions.FtpPattern = dto.FtpPatrn;
            jobOptions.TargetFileName = dto.TargetFileName;
        }

        private void MaptToHttpsOptions(RetrieverJobDto dto, HttpsOptions httpOptions)
        {
            httpOptions.Body = dto.HttpRequestBody;
            httpOptions.RequestMethod = dto.RequestMethod;
            httpOptions.RequestDataFormat = dto.RequestDataFormat;
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
        }
        #endregion
    }
}

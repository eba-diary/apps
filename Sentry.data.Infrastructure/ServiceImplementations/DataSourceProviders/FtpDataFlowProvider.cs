using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sentry.data.Infrastructure
{
    public class FtpDataFlowProvider : BaseJobProvider
    {
        private readonly IFtpProvider _ftpProvider;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFlowService _dataFlowService;
        private Submission _submission;

        public FtpDataFlowProvider(IFtpProvider ftpProvider, Lazy<IJobService> jobService, 
            IS3ServiceProvider s3ServiceProvider, IDataFlowService dataFlowService) : base(jobService)
        {
            _ftpProvider = ftpProvider;
            _s3ServiceProvider = s3ServiceProvider;
            _dataFlowService = dataFlowService;
        }
        public override void ConfigureProvider(RetrieverJob job)
        {
            job.JobLoggerMessage("Info", $"ftpdataflowprovider-configureprovider init ftp.job.options - ftppatter:{job.JobOptions.FtpPattern.ToString()} isregexsearch:{job.JobOptions.IsRegexSearch.ToString()} searchcriteria:{job.JobOptions.SearchCriteria}");
            _submission = _jobService.Value.SaveSubmission(job, "");
            _ftpProvider.SetCredentials(job.DataSource.SourceAuthType.GetCredentials(job));
        }

        public override void Execute(RetrieverJob job)
        {
            _job = job;

            ConfigureProvider(_job);

            try
            {
                switch (_job.JobOptions.FtpPattern)
                {
                    case FtpPattern.NoPattern:
                    default:
                        GenericFtpExecution(_jobService.Value.GetDataSourceUri(_job).AbsoluteUri);
                        break;
                    case FtpPattern.RegexFileNoDelete:
                        ProcessRegexFileNoDelete();
                        break;
                    case FtpPattern.RegexFileSinceLastExecution:
                        ProcessRegexFileSinceLastExecution();
                        break;
                    case FtpPattern.NewFilesSinceLastexecution:
                        ProcessNewFilesSinceLastExecution();
                        break;
                }
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", $"Retriever Job Failed", ex);
                _jobService.Value.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_FAILED_STATE);
            }
        }

        public override void Execute(RetrieverJob job, string filePath)
        {
            throw new NotImplementedException();
        }

        #region Private Methods
        private void GenericFtpExecution(string absoluteUri)
        {
            _jobService.Value.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);

            RetrieveFtpFile(absoluteUri);

            _jobService.Value.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE);
        }

        private void RetrieveFtpFile(string absoluteUri)
        {
            //Setup temporary work space for job
            var tempFile = SetupTempWorkSpace(absoluteUri);

            //Find the target prefix (s3) from S3DropAction on the DataFlow attached to RetrieverJob
            DataFlowStep s3DropStep = _dataFlowService.GetDataFlowStepForDataFlowByActionType(_job.DataFlow.Id, DataActionType.ProducerS3Drop);

            _job.JobLoggerMessage("Info", "Sending file to Temp location");

            try
            {
                using (Stream ftpstream = _ftpProvider.GetFileStream(absoluteUri))
                {
                    using (Stream filestream = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                    {
                        ftpstream.CopyTo(filestream);
                    }
                }
            }
            catch(RetrieverJobProcessingException ex)
            {
                _job.JobLoggerMessage("Error", "", ex);
                _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                //Cleanup temp file if exists
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
                throw;
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", "Retriever job failed streaming to temp location.", ex);
                _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                //Cleanup temp file if exists
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                throw;
            }

            _job.JobLoggerMessage("Info", "Sending file to S3 drop location");

            string targetkey = $"{s3DropStep.TriggerKey}{Path.GetFileName(absoluteUri)}";

            /******************************************************************************
            * Utilizing Trigger bucket since we want to trigger the targetStep identified
            ******************************************************************************/
            var versionId = _s3ServiceProvider.UploadDataFile(tempFile, s3DropStep.TriggerBucket, targetkey);

            _job.JobLoggerMessage("Info", $"File uploaded to S3 Drop Location  (Key:{s3DropStep.TargetBucket + "/" + targetkey} | VersionId:{versionId})");
        }

        private void ProcessRegexFileNoDelete()
        {
            _jobService.Value.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);

            string fileName = Path.GetFileName(_jobService.Value.GetDataSourceUri(_job).AbsoluteUri);
            if (fileName != "")
            {
                _job.JobLoggerMessage("Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }

            IList<RemoteFile> resultList = _ftpProvider.ListDirectoryContent(_jobService.Value.GetDataSourceUri(_job).AbsoluteUri, "files");

            _job.JobLoggerMessage("Info", $"specificfile.search search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_jobService.Value.GetDataSourceUri(_job).AbsoluteUri}");
            _job.JobLoggerMessage("Info", $"specificfile.search source.directory.count {resultList.Count.ToString()}");

            if (resultList.Any())
            {
                _job.JobLoggerMessage("Info", $"specificfile.search source.directory.content: {JsonConvert.SerializeObject(resultList)}");
            }

            var rx = new Regex(_job.JobOptions.SearchCriteria, RegexOptions.IgnoreCase);

            List<RemoteFile> matchList = new List<RemoteFile>();
            matchList = resultList.Where(w => rx.IsMatch(w.Name)).ToList();

            _job.JobLoggerMessage("Info", $"specificfile.search match.count {matchList.Count}");
            _job.JobLoggerMessage("Info", $"specificfile.search matchlist.content {JsonConvert.SerializeObject(matchList)}");

            foreach (RemoteFile file in matchList)
            {
                _job.JobLoggerMessage("Info", $"specificfile.search.processing.file {file.Name}");
                string remoteUrl = _jobService.Value.GetDataSourceUri(_job).AbsoluteUri + file.Name;
                RetrieveFtpFile(remoteUrl);
            }
        }

        private void ProcessRegexFileSinceLastExecution()
        {
            _jobService.Value.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);

            JobHistory lastExecution = _jobService.Value.GetLastExecution(_job);

            string fileName = Path.GetFileName(_jobService.Value.GetDataSourceUri(_job).AbsoluteUri);

            if (fileName != "")
            {
                _job.JobLoggerMessage("Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }

            IList<RemoteFile> resultList = new List<RemoteFile>();
            resultList = _ftpProvider.ListDirectoryContent(_jobService.Value.GetDataSourceUri(_job).AbsoluteUri, "files");

            _job.JobLoggerMessage("Info", $"regexlastexecution.search source.directory.count {resultList.Count.ToString()}");

            if (resultList.Any())
            {
                _job.JobLoggerMessage("Info", $"regexlastexecution.search source.directory.content: {JsonConvert.SerializeObject(resultList)}");
            }

            var rx = new Regex(_job.JobOptions.SearchCriteria, RegexOptions.IgnoreCase);

            List<RemoteFile> matchList = new List<RemoteFile>();

            if (lastExecution != null)
            {
                _job.JobLoggerMessage("Info", $"regexlastexecution.search executiontime:{lastExecution.Created.ToString("s")} search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_jobService.Value.GetDataSourceUri(_job).AbsoluteUri}");
                matchList = resultList.Where(w => rx.IsMatch(w.Name) && w.Modified > lastExecution.Created.AddSeconds(-10)).ToList();
            }
            else
            {
                _job.JobLoggerMessage("Info", $"regexlastexecution.search executiontime:noexecutionhistory search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_jobService.Value.GetDataSourceUri(_job).AbsoluteUri}");
                matchList = resultList.Where(w => rx.IsMatch(w.Name)).ToList();
            }

            _job.JobLoggerMessage("Info", $"regexlastexecution.search match.count {matchList.Count}");

            if (matchList.Any())
            {
                _job.JobLoggerMessage("Info", $"regexlastexecution.search matchlist.content: {JsonConvert.SerializeObject(matchList)}");
            }

            foreach (RemoteFile file in matchList)
            {
                _job.JobLoggerMessage("Info", $"regexlastexecution.search processing.file {file.Name}");
                string remoteUrl = _jobService.Value.GetDataSourceUri(_job).AbsoluteUri + file.Name;
                RetrieveFtpFile(remoteUrl);
            }

            _jobService.Value.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE);
        }

        private void ProcessNewFilesSinceLastExecution()
        {
            _jobService.Value.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);

            JobHistory lastExecution = _jobService.Value.GetLastExecution(_job);

            string fileName = Path.GetFileName(_jobService.Value.GetDataSourceUri(_job).AbsoluteUri);

            if (fileName != "")
            {
                _job.JobLoggerMessage("Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }
            IList<RemoteFile> resultList = new List<RemoteFile>();
            resultList = _ftpProvider.ListDirectoryContent(_jobService.Value.GetDataSourceUri(_job).AbsoluteUri, "files");

            _job.JobLoggerMessage("Info", $"newfileslastexecution.search source.directory.count {resultList.Count.ToString()}");

            if (resultList.Any())
            {
                _job.JobLoggerMessage("Info", $"newfileslastexecution.search source.directory.content: {JsonConvert.SerializeObject(resultList)}");
            }

            List<RemoteFile> matchList = new List<RemoteFile>();

            if (lastExecution != null)
            {
                _job.JobLoggerMessage("Info", $"newfileslastexecution.search executiontime:{lastExecution.Created.ToString("s")} sourcelocation:{_jobService.Value.GetDataSourceUri(_job).AbsoluteUri}");
                matchList = resultList.Where(w => w.Modified > lastExecution.Created.AddSeconds(-10)).ToList();
            }
            else
            {
                _job.JobLoggerMessage("Info", $"newfileslastexecution.search executiontime:noexecutionhistory sourcelocation:{_jobService.Value.GetDataSourceUri(_job).AbsoluteUri}");
                matchList = resultList.ToList();
            }

            _job.JobLoggerMessage("Info", $"newfileslastexecution.search match.count {matchList.Count}");

            if (matchList.Any())
            {
                _job.JobLoggerMessage("Info", $"newfileslastexecution.search matchlist.content: {JsonConvert.SerializeObject(matchList)}");
            }

            foreach (RemoteFile file in matchList)
            {
                _job.JobLoggerMessage("Info", $"newfileslastexecution.search processing.file {file.Name}");
                string remoteUrl = _jobService.Value.GetDataSourceUri(_job).AbsoluteUri + file.Name;
                RetrieveFtpFile(remoteUrl);
            }

            _jobService.Value.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE);
        }

        private string SetupTempWorkSpace(string fileName = null)
        {
            //Generates the following directory structure and file name
            // if filename passed
            //  <GoldeneyeWorkDir>/Jobs/<JobId>/<Unique Guid>/<Original File Name w/o extension>.txt
            // If filename not passed
            //  <GoldeneyeWorkDir>/Jobs/<JobId>/<Unique Guid>/<Unique Guid>.txt
            string tempFile;
            if (string.IsNullOrEmpty(fileName))
            {
                tempFile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Guid.NewGuid().ToString(), (Guid.NewGuid().ToString() + ".txt"));
            }
            else
            {
                tempFile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Guid.NewGuid().ToString(), (Path.GetFileNameWithoutExtension(fileName) + ".txt"));
            }

            //Create temp directory if exists
            Directory.CreateDirectory(Path.GetDirectoryName(tempFile));

            //Remove temp file if exists
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
            return tempFile;
        }
        #endregion
    }
}

using Microsoft.Extensions.Logging;
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
        private readonly IJobService _jobService;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IDataFlowService _dataFlowService;
        private Submission _submission;
        private readonly ILogger<FtpDataFlowProvider> _logger;

        public FtpDataFlowProvider(IFtpProvider ftpProvider, IJobService jobService, 
            IS3ServiceProvider s3ServiceProvider, IDataFlowService dataFlowService,
            ILogger<FtpDataFlowProvider> logger)
        {
            _ftpProvider = ftpProvider;
            _jobService = jobService;
            _s3ServiceProvider = s3ServiceProvider;
            _dataFlowService = dataFlowService;
            _logger = logger;
        }
        public override void ConfigureProvider(RetrieverJob job)
        {
            job.JobLoggerMessage(_logger,"Info", $"ftpdataflowprovider-configureprovider init ftp.job.options - ftppatter:{job.JobOptions.FtpPattern.ToString()} isregexsearch:{job.JobOptions.IsRegexSearch.ToString()} searchcriteria:{job.JobOptions.SearchCriteria}");
            _submission = _jobService.SaveSubmission(job, "");
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
                        GenericFtpExecution(_job.GetUri().AbsoluteUri);
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
                _job.JobLoggerMessage(_logger,"Error", $"Retriever Job Failed", ex);
                _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_FAILED_STATE);
            }
        }

        public override void Execute(RetrieverJob job, string filePath)
        {
            throw new NotImplementedException();
        }

        #region Private Methods
        private void GenericFtpExecution(string absoluteUri)
        {
            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);

            RetrieveFtpFile(absoluteUri);

            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE);
        }

        private void RetrieveFtpFile(string absoluteUri)
        {
            //Setup temporary work space for job
            var tempFile = SetupTempWorkSpace(absoluteUri);

            //Find the target prefix (s3) from S3DropAction on the DataFlow attached to RetrieverJob
            DataFlowStep s3DropStep = _dataFlowService.GetDataFlowStepForDataFlowByActionType(_job.DataFlow.Id, DataActionType.ProducerS3Drop);

            _job.JobLoggerMessage(_logger,"Info", "Sending file to Temp location");

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
                _job.JobLoggerMessage(_logger,"Error", "", ex);
                _job.JobLoggerMessage(_logger,"Info", "Performing FTP post-failure cleanup.");

                //Cleanup temp file if exists
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
                throw;
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage(_logger,"Error", "Retriever job failed streaming to temp location.", ex);
                _job.JobLoggerMessage(_logger,"Info", "Performing FTP post-failure cleanup.");

                //Cleanup temp file if exists
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                throw;
            }

            _job.JobLoggerMessage(_logger,"Info", "Sending file to S3 drop location");

            string targetkey = $"{s3DropStep.TriggerKey}{Path.GetFileName(absoluteUri)}";

            /******************************************************************************
            * Utilizing Trigger bucket since we want to trigger the targetStep identified
            ******************************************************************************/
            var versionId = _s3ServiceProvider.UploadDataFile(tempFile, s3DropStep.TriggerBucket, targetkey);

            _job.JobLoggerMessage(_logger,"Info", $"File uploaded to S3 Drop Location  (Key:{s3DropStep.TargetBucket + "/" + targetkey} | VersionId:{versionId})");
        }

        private void ProcessRegexFileNoDelete()
        {
            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);

            string fileName = Path.GetFileName(_job.GetUri().AbsoluteUri);
            if (fileName != "")
            {
                _job.JobLoggerMessage(_logger,"Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }

            IList<RemoteFile> resultList = _ftpProvider.ListDirectoryContent(_job.GetUri().AbsoluteUri, "files");

            _job.JobLoggerMessage(_logger,"Info", $"specificfile.search search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_job.GetUri().AbsoluteUri}");
            _job.JobLoggerMessage(_logger,"Info", $"specificfile.search source.directory.count {resultList.Count.ToString()}");

            if (resultList.Any())
            {
                _job.JobLoggerMessage(_logger,"Info", $"specificfile.search source.directory.content: {JsonConvert.SerializeObject(resultList)}");
            }

            var rx = new Regex(_job.JobOptions.SearchCriteria, RegexOptions.IgnoreCase);

            List<RemoteFile> matchList = new List<RemoteFile>();
            matchList = resultList.Where(w => rx.IsMatch(w.Name)).ToList();

            _job.JobLoggerMessage(_logger,"Info", $"specificfile.search match.count {matchList.Count}");
            _job.JobLoggerMessage(_logger,"Info", $"specificfile.search matchlist.content {JsonConvert.SerializeObject(matchList)}");

            foreach (RemoteFile file in matchList)
            {
                _job.JobLoggerMessage(_logger,"Info", $"specificfile.search.processing.file {file.Name}");
                string remoteUrl = _job.GetUri().AbsoluteUri + file.Name;
                RetrieveFtpFile(remoteUrl);
            }
        }

        private void ProcessRegexFileSinceLastExecution()
        {
            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);

            JobHistory lastExecution = _jobService.GetLastExecution(_job);

            string fileName = Path.GetFileName(_job.GetUri().AbsoluteUri);

            if (fileName != "")
            {
                _job.JobLoggerMessage(_logger,"Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }

            IList<RemoteFile> resultList = new List<RemoteFile>();
            resultList = _ftpProvider.ListDirectoryContent(_job.GetUri().AbsoluteUri, "files");

            _job.JobLoggerMessage(_logger,"Info", $"regexlastexecution.search source.directory.count {resultList.Count.ToString()}");

            if (resultList.Any())
            {
                _job.JobLoggerMessage(_logger,"Info", $"regexlastexecution.search source.directory.content: {JsonConvert.SerializeObject(resultList)}");
            }

            var rx = new Regex(_job.JobOptions.SearchCriteria, RegexOptions.IgnoreCase);

            List<RemoteFile> matchList = new List<RemoteFile>();

            if (lastExecution != null)
            {
                _job.JobLoggerMessage(_logger,"Info", $"regexlastexecution.search executiontime:{lastExecution.Created.ToString("s")} search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_job.GetUri().AbsoluteUri}");
                matchList = resultList.Where(w => rx.IsMatch(w.Name) && w.Modified > lastExecution.Created.AddSeconds(-10)).ToList();
            }
            else
            {
                _job.JobLoggerMessage(_logger,"Info", $"regexlastexecution.search executiontime:noexecutionhistory search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_job.GetUri().AbsoluteUri}");
                matchList = resultList.Where(w => rx.IsMatch(w.Name)).ToList();
            }

            _job.JobLoggerMessage(_logger,"Info", $"regexlastexecution.search match.count {matchList.Count}");

            if (matchList.Any())
            {
                _job.JobLoggerMessage(_logger,"Info", $"regexlastexecution.search matchlist.content: {JsonConvert.SerializeObject(matchList)}");
            }

            foreach (RemoteFile file in matchList)
            {
                _job.JobLoggerMessage(_logger,"Info", $"regexlastexecution.search processing.file {file.Name}");
                string remoteUrl = _job.GetUri().AbsoluteUri + file.Name;
                RetrieveFtpFile(remoteUrl);
            }

            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE);
        }

        private void ProcessNewFilesSinceLastExecution()
        {
            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);

            JobHistory lastExecution = _jobService.GetLastExecution(_job);

            string fileName = Path.GetFileName(_job.GetUri().AbsoluteUri);

            if (fileName != "")
            {
                _job.JobLoggerMessage(_logger,"Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }
            IList<RemoteFile> resultList = new List<RemoteFile>();
            resultList = _ftpProvider.ListDirectoryContent(_job.GetUri().AbsoluteUri, "files");

            _job.JobLoggerMessage(_logger,"Info", $"newfileslastexecution.search source.directory.count {resultList.Count.ToString()}");

            if (resultList.Any())
            {
                _job.JobLoggerMessage(_logger,"Info", $"newfileslastexecution.search source.directory.content: {JsonConvert.SerializeObject(resultList)}");
            }

            List<RemoteFile> matchList = new List<RemoteFile>();

            if (lastExecution != null)
            {
                _job.JobLoggerMessage(_logger,"Info", $"newfileslastexecution.search executiontime:{lastExecution.Created.ToString("s")} sourcelocation:{_job.GetUri().AbsoluteUri}");
                matchList = resultList.Where(w => w.Modified > lastExecution.Created.AddSeconds(-10)).ToList();
            }
            else
            {
                _job.JobLoggerMessage(_logger,"Info", $"newfileslastexecution.search executiontime:noexecutionhistory sourcelocation:{_job.GetUri().AbsoluteUri}");
                matchList = resultList.ToList();
            }

            _job.JobLoggerMessage(_logger,"Info", $"newfileslastexecution.search match.count {matchList.Count}");

            if (matchList.Any())
            {
                _job.JobLoggerMessage(_logger,"Info", $"newfileslastexecution.search matchlist.content: {JsonConvert.SerializeObject(matchList)}");
            }

            foreach (RemoteFile file in matchList)
            {
                _job.JobLoggerMessage(_logger,"Info", $"newfileslastexecution.search processing.file {file.Name}");
                string remoteUrl = _job.GetUri().AbsoluteUri + file.Name;
                RetrieveFtpFile(remoteUrl);
            }

            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE);
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

using Newtonsoft.Json;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public class FtpDataFlowProvider : BaseJobProvider
    {
        private readonly IFtpProvider _ftpProvider;
        private readonly IJobService _jobService;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private Submission _submission;

        public FtpDataFlowProvider(IFtpProvider ftpProvider, IJobService jobService, IS3ServiceProvider s3ServiceProvider)
        {
            _ftpProvider = ftpProvider;
            _jobService = jobService;
            _s3ServiceProvider = s3ServiceProvider;
        }
        public override void ConfigureProvider(RetrieverJob job)
        {
            job.JobLoggerMessage("Info", $"ftpdataflowprovider-configureprovider init ftp.job.options - ftppatter:{job.JobOptions.FtpPattern.ToString()} isregexsearch:{job.JobOptions.IsRegexSearch.ToString()} searchcriteria:{job.JobOptions.SearchCriteria}");
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
                    case FtpPattern.NoPattern: /* #0*/
                    default:
                        RetrieveFtpFile(_job.GetUri().AbsoluteUri);
                        break;
                    ////case FtpPattern.SpecificFileNoDelete:
                    ////    ProcessSpecificFileNoDelete();
                    ////    break;
                    case FtpPattern.RegexFileNoDelete:  /* #4*/
                        ProcessRegexFileNoDelete();
                        break;
                    ////case FtpPattern.SpecificFileArchive:  /* #5*/
                    ////    ProcessSpecificFileArchive(); 
                    ////    break;
                    //case FtpPattern.RegexFileSinceLastExecution:
                    //    ProcessRegexFileSinceLastExecution();
                    //    break;
                    //case FtpPattern.NewFilesSinceLastexecution:
                    //    ProcessNewFilesSinceLastExecution();
                    //    break;
                }
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", $"Retriever Job Failed", ex);
                _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_FAILED_STATE);
            }
        }

        public override void Execute(RetrieverJob job, string filePath)
        {
            throw new NotImplementedException();
        }

        #region Private MethodsS
        private void RetrieveFtpFile(string absoluteUri)
        {
            //Setup temporary work space for job
            var tempFile = SetupTempWorkSpace(absoluteUri);

            //Find the target prefix (s3) from S3DropAction on the DataFlow attached to RetrieverJob
            DataFlowStep s3DropStep = _job.DataFlow.Steps.Where(w => w.DataAction_Type_Id == Core.Entities.DataProcessing.DataActionType.S3Drop).FirstOrDefault();


            _job.JobLoggerMessage("Info", "Sending file to S3 drop location");

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
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", "Retriever job failed streaming temp location.", ex);
                _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                //Cleanup temp file if exists
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                throw;
            }
            
            string targetkey = $"{s3DropStep.TargetPrefix}{_job.GetTargetFileName(Path.GetFileName(_job.GetUri().ToString()))}";
            var versionId = _s3ServiceProvider.UploadDataFile(tempFile, targetkey);

            _job.JobLoggerMessage("Info", $"File uploaded to S3 Drop Location  (Key:{targetkey} | VersionId:{versionId})");
        }

        private void ProcessRegexFileNoDelete()
        {
            string fileName = Path.GetFileName(_job.GetUri().AbsoluteUri);
            if (fileName != "")
            {
                _job.JobLoggerMessage("Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }

            IList<RemoteFile> resultList = _ftpProvider.ListDirectoryContent(_job.GetUri().AbsoluteUri, "files");

            _job.JobLoggerMessage("Info", $"specificfile.search search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_job.GetUri().AbsoluteUri}");
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
                string remoteUrl = _job.GetUri().AbsoluteUri + file.Name;
                RetrieveFtpFile(remoteUrl);
            }
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

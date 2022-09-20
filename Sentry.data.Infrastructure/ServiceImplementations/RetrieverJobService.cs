using Hangfire;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class RetrieverJobService : IRetrieverJobService
    {
        private RetrieverJob _job;
        private Submission _submission;
        private IFtpProvider _ftpProvider;
        private IDatasetContext _requestContext;
        private IJobService _jobService;
        private readonly IDatasetContext _datasetContext;
        private readonly List<KeyValuePair<string, string>> _dropLocationTags = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("ProcessingStatus","NotStarted")
        };

        public RetrieverJobService(IJobService jobService, IDatasetContext datasetContext)
        {
            _jobService = jobService;
            _datasetContext = datasetContext;
        }
        /// <summary>
        /// Implementation of core job logic for each DataSOurce type.
        /// </summary>
        /// <param name="JobId">ID of the retriever job</param>
        /// <param name="filePath">(For DFSBasic jobs) File name, including extension, to process.  If null is passed then will process all files within directory</param>
        public void RunRetrieverJob(int JobId, IJobCancellationToken token, string filePath = null)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                using (IContainer Container = Bootstrapper.Container.GetNestedContainer())
                {
                    _requestContext = Container.GetInstance<IDatasetContext>();
                    _jobService = Container.GetInstance<IJobService>();

                    //Retrieve job details
                    _job = _requestContext.RetrieverJob.Where(w => w.Id == JobId).FetchAllConfiguration(_requestContext).FirstOrDefault();
                    IBaseJobProvider _jobProvider;
                    //if logic only needed until all sources are converted to this Source\Provider pattern

                    //verify if job is configured for new provider pattern, otherwise handled via legacy code.
                    if (_job.DataFlow == null)
                    {
                        switch (_job.DataSource.SourceType)
                        {
                            case GlobalConstants.DataSoureDiscriminator.GOOGLE_API_SOURCE:
                            case GlobalConstants.DataSoureDiscriminator.HTTPS_SOURCE:
                            case GlobalConstants.DataSoureDiscriminator.FTP_DATAFLOW_SOURCE:
                                _jobProvider = Container.GetInstance<IBaseJobProvider>(_job.DataSource.SourceType);

                                // Execute job
                                if (_jobProvider != null)
                                {
                                    _jobProvider.Execute(_job);
                                }
                                break;
                            default:
                                ExecuteLegacyProcessing(filePath);
                                break;
                        }
                    }
                    else
                    {
                        bool includeFilename = false;
                        switch (_job.DataSource.SourceType)
                        {
                            //Map exising source type to new Dataflow Provider
                            case GlobalConstants.DataSoureDiscriminator.FTP_SOURCE:
                                _jobProvider = Container.GetInstance<IBaseJobProvider>(GlobalConstants.DataSoureDiscriminator.FTP_DATAFLOW_SOURCE);
                                break;
                            case GlobalConstants.DataSoureDiscriminator.GOOGLE_API_SOURCE:
                                _jobProvider = Container.GetInstance<IBaseJobProvider>(GlobalConstants.DataSoureDiscriminator.GOOGLE_API_DATAFLOW_SOURCE);
                                break;
                            case GlobalConstants.DataSoureDiscriminator.HTTPS_SOURCE:
                                _jobProvider = Container.GetInstance<IBaseJobProvider>(GlobalConstants.DataSoureDiscriminator.GENERIC_HTTPS_DATAFLOW_SOURCE);
                                break;
                            case GlobalConstants.DataSoureDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION:
                                includeFilename = true;
                                _jobProvider = Container.GetInstance<IBaseJobProvider>(_job.DataSource.SourceType);
                                break;
                            default:
                                _job.JobLoggerMessage("Debug", $"RetrieverJobService not configured for source type  - jobid:{_job.Id} sourcetype:{_job.DataSource.SourceType}");
                                throw new NotImplementedException($"RetrieverJobService not configured for source type - jobid:{_job.Id} sourcetype:{_job.DataSource.SourceType}");
                        }

                        if (_jobProvider != null)
                        {
                            if (includeFilename)
                            {
                                _jobProvider.Execute(_job, filePath);
                            }
                            else
                            {
                                _jobProvider.Execute(_job);
                            }
                        }
                    }
                    #region OldLegacyCode

                    //if (_job.DataSource.Is<FtpSource>())
                    //{
                    //    _submission = _jobService.SaveSubmission(_job, "");

                    //    _ftpProvider = Container.GetInstance<IFtpProvider>();
                    //    _ftpProvider.SetCredentials(_job.DataSource.SourceAuthType.GetCredentials(_job));

                    //    _job.JobLoggerMessage("Info", $"ftp.job.options - ftppatter:{_job.JobOptions.FtpPattern.ToString()} isregexsearch:{_job.JobOptions.IsRegexSearch.ToString()} searchcriteria:{_job.JobOptions.SearchCriteria}");

                    //    try
                    //    {
                    //        switch (_job.JobOptions.FtpPattern)
                    //        {
                    //            case 
                    //            FtpPattern.NoPattern: /* #0*/
                    //            default:
                    //                RetrieveFTPFile(_job.GetUri().AbsoluteUri);                                    
                    //                break;
                    //            //case FtpPattern.SpecificFileNoDelete:
                    //            //    ProcessSpecificFileNoDelete();
                    //            //    break;
                    //            case FtpPattern.RegexFileNoDelete:  /* #4*/
                    //                ProcessRegexFileNoDelete(); 
                    //                break;
                    //            //case FtpPattern.SpecificFileArchive:  /* #5*/
                    //            //    ProcessSpecificFileArchive(); 
                    //            //    break;
                    //            case FtpPattern.RegexFileSinceLastExecution:
                    //                ProcessRegexFileSinceLastExecution();
                    //                break;
                    //            case FtpPattern.NewFilesSinceLastexecution:
                    //                ProcessNewFilesSinceLastExecution();
                    //                break;
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        _job.JobLoggerMessage("Error", $"Retriever Job Failed", ex);
                    //        _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_FAILED_STATE);
                    //    }                        
                    //}
                    //else if (_job.DataSource.Is<SFtpSource>())
                    //{
                    //    //WinSCP (dll used for SFTP Transfers) does not expose file transfer as a stream.  When transfering to S3, file will be transfered
                    //    //  to a local temp folder, then streamed to the S3 drop location.  For DfsBasic targets, the file will be transfered
                    //    //  directly to the DfsBasic GetUri() location.
                    //    try
                    //    {
                    //        SftpProvider _sftpProvider = new SftpProvider();
                    //        var tempFile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Path.GetFileName(_job.GetUri().ToString()));

                    //        //If source file is compressed, need to save to temp location and send job to JAWS
                    //        if (_job.JobOptions != null && _job.JobOptions.CompressionOptions.IsCompressed)
                    //        {
                    //            _job.JobLoggerMessage("Info", $"Compressed option is detected... Streaming to temp location");

                    //            //Create temp directory if exists
                    //            _job.JobLoggerMessage("Info", $"Creating temp work directory - Directory:{Path.GetDirectoryName(tempFile)}");
                    //            Directory.CreateDirectory(Path.GetDirectoryName(tempFile));

                    //            //Delete tempFile if exits
                    //            if (File.Exists(tempFile)) {
                    //                File.Delete(tempFile);
                    //            }

                    //            try
                    //            {
                    //                _sftpProvider.GetFile(_job, tempFile);

                    //                //Create a fire-forget Hangfire job to decompress the file and drop extracted file into drop locations
                    //                BackgroundJob.Enqueue<JawsService>(x => x.UncompressRetrieverJob(_job.Id, tempFile));
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                    //                _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                    //                //Cleanup any files created
                    //                if (File.Exists(tempFile))
                    //                {
                    //                    File.Delete(tempFile);
                    //                }
                    //            }

                    //        }
                    //        else
                    //        {
                    //            //Find the DFSBasic job for the config the SFTP Job is assigned too.  Use the GetUri() from the DFSBasic job as target path, combined with the SFTP job GetTargetFileName().
                    //            RetrieverJob TargetJob = null;
                    //            string targetpath = null;
                    //            TargetJob = _requestContext.RetrieverJob.FirstOrDefault(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && w.DataSource is S3Basic);

                    //            // If an S3Basic job was not found, find the DfsBasic job to drop the file
                    //            if (TargetJob == null)
                    //            {
                    //                _job.JobLoggerMessage("Info", "No S3Basic job found for Schema... Finding DfsBasic job");
                    //                TargetJob = _requestContext.RetrieverJob.FirstOrDefault(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && w.DataSource is DfsBasic);

                    //                if (TargetJob == null)
                    //                {
                    //                    throw new NotImplementedException("Failed to find generic S3 Basic job");
                    //                }

                    //                _job.JobLoggerMessage("Info", "Found DfsBasic job for schema");
                    //                targetpath = Path.Combine(TargetJob.GetUri().LocalPath, _job.GetTargetFileName(Path.GetFileName(_job.GetUri().ToString())));

                    //                try
                    //                {
                    //                    _sftpProvider.GetFile(_job, targetpath);
                    //                }
                    //                catch (Exception ex)
                    //                {
                    //                    _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                    //                    _job.JobLoggerMessage("Info", "Performing SFTP post-failure cleanup.");

                    //                    //Cleanup any files created
                    //                    if (File.Exists(targetpath))
                    //                    {
                    //                        File.Delete(targetpath);
                    //                    }
                    //                }

                    //            }
                    //            else
                    //            {
                    //                _job.JobLoggerMessage("Info", "Found S3Basic job for schema");

                    //                try
                    //                {
                    //                    _sftpProvider.GetFile(_job, targetpath);
                    //                }
                    //                catch (Exception ex)
                    //                {
                    //                    _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                    //                    _job.JobLoggerMessage("Info", "Performing SFTP post-failure cleanup.");

                    //                    //Cleanup any files created
                    //                    if (File.Exists(targetpath))
                    //                    {
                    //                        File.Delete(targetpath);
                    //                    }
                    //                }


                    //                S3ServiceProvider s3Service = new S3ServiceProvider();
                    //                string targetkey = $"{TargetJob.DataSource.GetDropPrefix(TargetJob)}{_job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath))}";
                    //                var versionId = s3Service.UploadDataFile(tempFile, targetkey);

                    //                _job.JobLoggerMessage("Info", $"File uploaded to S3 Drop Location  (Key:{targetkey} | VersionId:{versionId})");
                    //            }                                
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        _job.JobLoggerMessage("Error", "Retriever Job Failed", ex);
                    //    }
                    //}
                    //else if (_job.DataSource.Is<DfsBasic>())
                    //{
                    //    try
                    //    {
                    //        //This logic will only be hit when a DFSBasic job has a schedule defined instead of "Instant" (directory monitory executed via watch.cs)

                    //        //Set directory search
                    //        var dirSearchCriteria = (String.IsNullOrEmpty(filePath)) ? "*" : filePath;

                    //        //Only search top directory and source files not locked and does not start with two exclamaition points !!
                    //        foreach (var a in Directory.GetFiles(_job.GetUri().LocalPath, dirSearchCriteria, SearchOption.TopDirectoryOnly).Where(w => !IsFileLocked(w) && !Path.GetFileName(w).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix"))))
                    //        {
                    //            if (_job.JobOptions.CompressionOptions.IsCompressed)
                    //            {
                    //                //TODO: Revisit delete source file logic to handle not deleting source file
                    //                ProcessCompressedFile(a, true);
                    //            }
                    //            else
                    //            {
                    //                //check for searchcriteria for filtering incoming files
                    //                if (!_job.FilterIncomingFile(Path.GetFileName(a)))
                    //                {
                    //                    //Submit Loader Request
                    //                    SubmitLoaderRequest(a);
                    //                }
                    //                else
                    //                {
                    //                    _job.JobLoggerMessage("Info", $"Filtered file from processing ({a})");
                    //                }
                    //            }
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        _job.JobLoggerMessage("Error", $"Retriever Job Failed", ex);                            
                    //    }                        
                    //}
                    //else if (_job.DataSource.Is<DfsCustom>())
                    //{
                    //    //Set directory search
                    //    var dirSearchCriteria = (String.IsNullOrEmpty(filePath)) ? "*" : filePath;

                    //    //Only search top directory and source files not locked and does not start with two exclamaition points !!
                    //    foreach (var b in Directory.GetFiles(_job.GetUri().LocalPath, dirSearchCriteria, SearchOption.TopDirectoryOnly).Where(w => !IsFileLocked(w) && !Path.GetFileName(w).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix"))))
                    //    {                            
                    //        if (_job.JobOptions.CompressionOptions.IsCompressed)
                    //        {
                    //            //File filtering will take place on decompressed files
                    //            //TODO: Revisit delete source file logic to handle not deleting source file
                    //            ProcessCompressedFile(b, true);
                    //        }
                    //        else
                    //        {
                    //            //check for searchcriteria for filtering incoming files
                    //            if (!_job.FilterIncomingFile(Path.GetFileName(b)))
                    //            {
                    //                //Submit Loader Request
                    //                SubmitLoaderRequest(b);
                    //            }
                    //            else
                    //            {
                    //                _job.JobLoggerMessage("Info", $"Filtered file from processing ({b})");
                    //            }
                    //        }                            
                    //    }
                    //}
                    //else if (_job.DataSource.Is<S3Basic>())
                    //{
                    //    S3ServiceProvider s3Service = new S3ServiceProvider();

                    //    List<string> objectList = new List<string>();

                    //    //filter any keys which contain a ProcessingStatus tag
                    //    foreach (string key in s3Service.ListObjects(_job.DataSource.Bucket, _job.DataSource.GetDropPrefix(_job)))
                    //    {
                    //        if(!s3Service.GetObjectTags(_job.DataSource.Bucket, key).Any(w => w.Key == "ProcessingStatus"))
                    //        {
                    //            //Add ProcessingStatus tag to signify object is being process and subsequent S3BasicJobs do not pick up file for processing
                    //            s3Service.AddObjectTag(_job.DataSource.Bucket, key, _dropLocationTags);
                    //            objectList.Add(key);
                    //        }
                    //    }

                    //    if (objectList == null)
                    //    {
                    //        //_job.JobLoggerMessage("Info", "S3 Basic job detected 0 new files");
                    //    }
                    //    else
                    //    {
                    //        if (objectList.Count > 0)
                    //        {
                    //            _job.JobLoggerMessage("Info", $"S3 Basic job detected {objectList.Count.ToString()} new files");
                    //        }

                    //        foreach (string a in objectList)
                    //        {
                    //            if (_job.JobOptions.CompressionOptions.IsCompressed)
                    //            {
                    //                //File filtering takes place on decompressed files
                    //                //TODO: Revisit delete source file logic to handle not deleting source file
                    //                ProcessCompressedFile(a, true);
                    //            }
                    //            else
                    //            {
                    //           if (!_job.FilterIncomingFile(Path.GetFileName(a)))
                    //                {
                    //                    //Submit Loader Request
                    //                    SubmitLoaderRequest(a);
                    //                }
                    //                else
                    //                {
                    //                    _job.JobLoggerMessage("Info", $"Filtered file from processing ({a})");
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Info($"Retriever Job Cancelled - Job:{JobId}");
                throw;
            }
            catch (RetrieverJobProcessingException ex)
            {
                Logger.Error($"Retriever Job Failed - Job:{JobId} processing_failed", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"Retriever Job Failed to initialize - Job:{JobId}", ex);
            }
        }

        /// <summary>
        /// Used to run HSZ retriever jobs
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="token"></param>
        /// <param name="filePath"></param>
        public void RunHszRetrieverJob(RetrieverJob job, CancellationToken token, string filePath = null)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                using (IContainer Container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                {
                    _requestContext = Container.GetInstance<IDatasetContext>();
                    IS3ServiceProvider _s3ServiceProvider = Container.GetInstance<IS3ServiceProvider>();

                    //set job details
                    _job = job;

                    try
                    {
                        RetrieverJob targetJob = _requestContext.RetrieverJob.Fetch(f => f.DatasetConfig).ThenFetch(d => d.ParentDataset).Fetch(f => f.DataSource).FirstOrDefault(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && w.DataSource is S3Basic);
                        //Get target path based on basic job found
                        //string targetFullPath = GetTargetPath(targetJob);

                        //Set directory search
                        var dirSearchCriteria = (String.IsNullOrEmpty(filePath)) ? "*" : Path.GetFileName(filePath);

                        //Only search top directory and source files not locked and does not start with two exclamaition points !!
                        foreach (var a in Directory.GetFiles(_job.GetUri().LocalPath, dirSearchCriteria, SearchOption.TopDirectoryOnly).Where(w => !IsFileLocked(w) && !Path.GetFileName(w).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix"))))
                        {
                            if (_job.JobOptions.CompressionOptions.IsCompressed)
                            {
                                //TODO: Revisit delete source file logic to handle not deleting source file
                                ProcessCompressedFile(a, true);
                            }
                            else
                            {
                                //check for searchcriteria for filtering incoming files
                                if (!_job.FilterIncomingFile(Path.GetFileName(a)))
                                {

                                    string processingFile = GenerateProcessingFileName(filePath);

                                    //Rename file to indicate a request has been sent to Dataset Loader
                                    File.Move(filePath, processingFile);

                                    //generate targetkey, remove processing indicator from filename
                                    string targetkey = $"{targetJob.DataSource.GetDropPrefix(targetJob)}{_job.GetTargetFileName(Path.GetFileName(filePath).Replace(Configuration.Config.GetHostSetting("ProcessedFilePrefix"),""))}";
                                    var versionId = _s3ServiceProvider.UploadDataFile(processingFile, targetkey);

                                    //Cleanup target file if exists
                                    if (File.Exists(processingFile))
                                    {
                                        File.Delete(processingFile);
                                    }

                                    _job.JobLoggerMessage("Info", $"File uploaded to S3 Drop Location  (Key:{targetkey} | VersionId:{versionId})");

                                }
                                else
                                {
                                    _job.JobLoggerMessage("Info", $"Filtered file from processing ({a})");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _job.JobLoggerMessage("Error", $"Retriever Job Failed", ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Info($"Retriever Job Cancelled - Job:{job.Id}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Retriever Job Failed to initialize - Job:{job.Id}", ex);
            }
        }
        
        private void ExecuteLegacyProcessing(string filePath)
        {
            
                if (_job.DataSource.Is<FtpSource>())
                {
                    using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                    {
                        _submission = _jobService.SaveSubmission(_job, "");

                        _ftpProvider = container.GetInstance<IFtpProvider>();
                        _ftpProvider.SetCredentials(_job.DataSource.SourceAuthType.GetCredentials(_job));

                        _job.JobLoggerMessage("Info", $"ftp.job.options - ftppatter:{_job.JobOptions.FtpPattern.ToString()} isregexsearch:{_job.JobOptions.IsRegexSearch.ToString()} searchcriteria:{_job.JobOptions.SearchCriteria}");

                        try
                        {
                            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_STARTED_STATE);
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
                            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_SUCCESS_STATE);
                        }
                        catch (Exception ex)
                        {
                            _job.JobLoggerMessage("Error", $"Retriever Job Failed", ex);
                            _jobService.RecordJobState(_submission, _job, GlobalConstants.JobStates.RETRIEVERJOB_FAILED_STATE);
                        }
                    }
                }
                else if (_job.DataSource.Is<SFtpSource>())
                {
                    //WinSCP (dll used for SFTP Transfers) does not expose file transfer as a stream.  When transfering to S3, file will be transfered
                    //  to a local temp folder, then streamed to the S3 drop location.  For DfsBasic targets, the file will be transfered
                    //  directly to the DfsBasic GetUri() location.
                    try
                    {
                        SftpProvider _sftpProvider = new SftpProvider();
                        var tempFile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Path.GetFileName(_job.GetUri().ToString()));

                        //If source file is compressed, need to save to temp location and send job to JAWS
                        if (_job.JobOptions != null && _job.JobOptions.CompressionOptions.IsCompressed)
                        {
                            _job.JobLoggerMessage("Info", $"Compressed option is detected... Streaming to temp location");

                            //Create temp directory if exists
                            _job.JobLoggerMessage("Info", $"Creating temp work directory - Directory:{Path.GetDirectoryName(tempFile)}");
                            Directory.CreateDirectory(Path.GetDirectoryName(tempFile));

                            //Delete tempFile if exits
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                            }

                            try
                            {
                                _sftpProvider.GetFile(_job, tempFile);

                                //Create a fire-forget Hangfire job to decompress the file and drop extracted file into drop locations
                                BackgroundJob.Enqueue<JawsService>(x => x.UncompressRetrieverJob(_job.Id, tempFile));
                            }
                            catch (Exception ex)
                            {
                                _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                                _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                                //Cleanup any files created
                                if (File.Exists(tempFile))
                                {
                                    File.Delete(tempFile);
                                }
                            }

                        }
                        else
                        {
                            //Find the DFSBasic job for the config the SFTP Job is assigned too.  Use the GetUri() from the DFSBasic job as target path, combined with the SFTP job GetTargetFileName().
                            RetrieverJob TargetJob = null;
                            string targetpath = null;
                            TargetJob = _requestContext.RetrieverJob.FirstOrDefault(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && w.DataSource is S3Basic);

                            // If an S3Basic job was not found, find the DfsBasic job to drop the file
                            if (TargetJob == null)
                            {
                                _job.JobLoggerMessage("Info", "No S3Basic job found for Schema... Finding DfsBasic job");
                                TargetJob = _requestContext.RetrieverJob.FirstOrDefault(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && w.DataSource is DfsBasic);

                                if (TargetJob == null)
                                {
                                    throw new NotImplementedException("Failed to find generic S3 Basic job");
                                }

                                _job.JobLoggerMessage("Info", "Found DfsBasic job for schema");
                                targetpath = Path.Combine(TargetJob.GetUri().LocalPath, _job.GetTargetFileName(Path.GetFileName(_job.GetUri().ToString())));

                                try
                                {
                                    _sftpProvider.GetFile(_job, targetpath);
                                }
                                catch (Exception ex)
                                {
                                    _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                                    _job.JobLoggerMessage("Info", "Performing SFTP post-failure cleanup.");

                                    //Cleanup any files created
                                    if (File.Exists(targetpath))
                                    {
                                        File.Delete(targetpath);
                                    }
                                }

                            }
                            else
                            {
                                _job.JobLoggerMessage("Info", "Found S3Basic job for schema");

                                try
                                {
                                    _sftpProvider.GetFile(_job, targetpath);
                                }
                                catch (Exception ex)
                                {
                                    _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                                    _job.JobLoggerMessage("Info", "Performing SFTP post-failure cleanup.");

                                    //Cleanup any files created
                                    if (File.Exists(targetpath))
                                    {
                                        File.Delete(targetpath);
                                    }
                                }


                                S3ServiceProvider s3Service = new S3ServiceProvider();
                                string targetkey = $"{TargetJob.DataSource.GetDropPrefix(TargetJob)}{_job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath))}";
                                var versionId = s3Service.UploadDataFile(tempFile, targetkey);

                                _job.JobLoggerMessage("Info", $"File uploaded to S3 Drop Location  (Key:{targetkey} | VersionId:{versionId})");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _job.JobLoggerMessage("Error", "Retriever Job Failed", ex);
                    }
                }
                else if (_job.DataSource.Is<DfsBasic>())
                {
                    try
                    {
                        //This logic will only be hit when a DFSBasic job has a schedule defined instead of "Instant" (directory monitory executed via watch.cs)

                        //Set directory search
                        var dirSearchCriteria = (String.IsNullOrEmpty(filePath)) ? "*" : filePath;

                        //Only search top directory and source files not locked and does not start with two exclamaition points !!
                        foreach (var a in Directory.GetFiles(_job.GetUri().LocalPath, dirSearchCriteria, SearchOption.TopDirectoryOnly).Where(w => !IsFileLocked(w) && !Path.GetFileName(w).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix"))))
                        {
                            if (_job.JobOptions.CompressionOptions.IsCompressed)
                            {
                                //TODO: Revisit delete source file logic to handle not deleting source file
                                ProcessCompressedFile(a, true);
                            }
                            else
                            {
                                //check for searchcriteria for filtering incoming files
                                if (!_job.FilterIncomingFile(Path.GetFileName(a)))
                                {
                                    //Submit Loader Request
                                    SubmitLoaderRequest(a);
                                }
                                else
                                {
                                    _job.JobLoggerMessage("Info", $"Filtered file from processing ({a})");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _job.JobLoggerMessage("Error", $"Retriever Job Failed", ex);
                    }
                }
                else if (_job.DataSource.Is<DfsCustom>())
                {
                    //Set directory search
                    var dirSearchCriteria = (String.IsNullOrEmpty(filePath)) ? "*" : filePath;

                    //Only search top directory and source files not locked and does not start with two exclamaition points !!
                    foreach (var b in Directory.GetFiles(_job.GetUri().LocalPath, dirSearchCriteria, SearchOption.TopDirectoryOnly).Where(w => !IsFileLocked(w) && !Path.GetFileName(w).StartsWith(Configuration.Config.GetHostSetting("ProcessedFilePrefix"))))
                    {
                        if (_job.JobOptions.CompressionOptions.IsCompressed)
                        {
                            //File filtering will take place on decompressed files
                            //TODO: Revisit delete source file logic to handle not deleting source file
                            ProcessCompressedFile(b, true);
                        }
                        else
                        {
                            //check for searchcriteria for filtering incoming files
                            if (!_job.FilterIncomingFile(Path.GetFileName(b)))
                            {
                                //Submit Loader Request
                                SubmitLoaderRequest(b);
                            }
                            else
                            {
                                _job.JobLoggerMessage("Info", $"Filtered file from processing ({b})");
                            }
                        }
                    }
                }
                else if (_job.DataSource.Is<S3Basic>())
                {
                    S3ServiceProvider s3Service = new S3ServiceProvider();

                    List<string> objectList = new List<string>();

                    //filter any keys which contain a ProcessingStatus tag
                    foreach (string key in s3Service.ListObjects(_job.DataSource.Bucket, _job.DataSource.GetDropPrefix(_job)))
                    {
                        if (!s3Service.GetObjectTags(_job.DataSource.Bucket, key).Any(w => w.Key == "ProcessingStatus"))
                        {
                            //Add ProcessingStatus tag to signify object is being process and subsequent S3BasicJobs do not pick up file for processing
                            s3Service.AddObjectTag(_job.DataSource.Bucket, key, _dropLocationTags);
                            objectList.Add(key);
                        }
                    }

                    if (objectList == null)
                    {
                        //_job.JobLoggerMessage("Info", "S3 Basic job detected 0 new files");
                    }
                    else
                    {
                        if (objectList.Count > 0)
                        {
                            _job.JobLoggerMessage("Info", $"S3 Basic job detected {objectList.Count.ToString()} new files");
                        }

                        foreach (string a in objectList)
                        {
                            if (_job.JobOptions.CompressionOptions.IsCompressed)
                            {
                                //File filtering takes place on decompressed files
                                //TODO: Revisit delete source file logic to handle not deleting source file
                                ProcessCompressedFile(a, true);
                            }
                            else
                            {
                                if (!_job.FilterIncomingFile(Path.GetFileName(a)))
                                {
                                    //Submit Loader Request
                                    SubmitLoaderRequest(a);
                                }
                                else
                                {
                                    _job.JobLoggerMessage("Info", $"Filtered file from processing ({a})");
                                }
                            }
                        }
                    }
                }
        }

        #region FTP Processing
        private void GenericFtpExecution(string uri)
        {
            RetrieveFTPFile(uri);
        }

        private void ProcessNewFilesSinceLastExecution()
        {
            JobHistory lastExecution = _jobService.GetLastExecution(_job);

            string fileName = Path.GetFileName(_job.GetUri().AbsoluteUri);

            if (fileName != "")
            {
                _job.JobLoggerMessage("Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }
            IList<RemoteFile> resultList = new List<RemoteFile>();
            resultList = _ftpProvider.ListDirectoryContent(_job.GetUri().AbsoluteUri, "files");

            _job.JobLoggerMessage("Info", $"newfileslastexecution.search source.directory.count {resultList.Count.ToString()}");

            if (resultList.Any())
            {
                _job.JobLoggerMessage("Info", $"newfileslastexecution.search source.directory.content: {JsonConvert.SerializeObject(resultList)}");
            }                

            List<RemoteFile> matchList = new List<RemoteFile>();

            if (lastExecution != null)
            {
                _job.JobLoggerMessage("Info", $"newfileslastexecution.search executiontime:{lastExecution.Created.ToString("s")} sourcelocation:{_job.GetUri().AbsoluteUri}");
                matchList = resultList.Where(w => w.Modified > lastExecution.Created.AddSeconds(-10)).ToList();
            }
            else
            {
                _job.JobLoggerMessage("Info", $"newfileslastexecution.search executiontime:noexecutionhistory sourcelocation:{_job.GetUri().AbsoluteUri}");
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
                string remoteUrl = _job.GetUri().AbsoluteUri + file.Name;
                RetrieveFTPFile(remoteUrl);
            }
        }

        private void ProcessRegexFileSinceLastExecution()
        {
            JobHistory lastExecution = _jobService.GetLastExecution(_job);

            string fileName = Path.GetFileName(_job.GetUri().AbsoluteUri);

            if (fileName != "")
            {
                _job.JobLoggerMessage("Error", "Job terminating - Uri does not end with forward slash.");
                return;
            }

            IList<RemoteFile> resultList = new List<RemoteFile>();
            resultList = _ftpProvider.ListDirectoryContent(_job.GetUri().AbsoluteUri, "files");

            _job.JobLoggerMessage("Info", $"regexlastexecution.search source.directory.count {resultList.Count.ToString()}");

            if (resultList.Any())
            {
                _job.JobLoggerMessage("Info", $"regexlastexecution.search source.directory.content: {JsonConvert.SerializeObject(resultList)}");
            }

            var rx = new Regex(_job.JobOptions.SearchCriteria, RegexOptions.IgnoreCase);

            List<RemoteFile> matchList = new List<RemoteFile>();

            if (lastExecution != null)
            {
                _job.JobLoggerMessage("Info", $"regexlastexecution.search executiontime:{lastExecution.Created.ToString("s")} search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_job.GetUri().AbsoluteUri}");
                matchList = resultList.Where(w => rx.IsMatch(w.Name) && w.Modified > lastExecution.Created.AddSeconds(-10)).ToList();
            }
            else
            {
                _job.JobLoggerMessage("Info", $"regexlastexecution.search executiontime:noexecutionhistory search.regex:{_job.JobOptions.SearchCriteria} sourcelocation:{_job.GetUri().AbsoluteUri}");
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
                string remoteUrl = _job.GetUri().AbsoluteUri + file.Name;
                RetrieveFTPFile(remoteUrl);
            }
        }

        //private void ProcessSpecificFileArchive()
        //{
        //    //throw new NotImplementedException();
        //    //Process specific file
        //    ProcessSpecificFileNoDelete();

        //    //Archive specific file
        //    ArchiveSpecificFile(_job.GetUri().AbsoluteUri);

        //}

        //private void ArchiveSpecificFile(string sourceUrl)
        //{
        //    //throw new NotImplementedException();
        //    //Remove filename from job URI
        //    string baseDir = sourceUrl.Replace(Path.GetFileName(sourceUrl), "");

        //    //Add archive directory name to URI
        //    string archiveDir = baseDir + "archive/";

        //    //Determine if Archive directory exists
        //    IList<RemoteFile> resultList = _ftpProvider.ListDirectoryContent(baseDir, "directories");

        //    if (!resultList.Where(w => w.Name == "archive").Any())
        //    {
        //        _job.JobLoggerMessage("Error", $"FTP archive directory does not exist, ask vendor to create archive directory in url - url:{archiveDir}");
        //    }

        //    _ftpProvider.RenameFile(sourceUrl, "ftp://ftp.cabfinancial.com/PublicData/PublicData20121231/ISS_History_Test.zip");
        //}

        //private void ProcessSpecificFileNoDelete()
        //{
        //    string remoteDir = null;

        //    string fileName = Path.GetFileName(_job.GetUri().AbsoluteUri);
        //    if (fileName != "")
        //    {
        //        remoteDir = _job.GetUri().AbsoluteUri.Replace(fileName, "");
        //    }
        //    else
        //    {
        //        _job.JobLoggerMessage("Error", "Job terminating - Uri does not contain file name.");
        //        throw new ArgumentNullException("RelativeUri", "Uri does not contain file name");
        //    }

        //    IList<RemoteFile> resultList = _ftpProvider.ListDirectoryContent(remoteDir, "files");

        //    if (resultList.Where(w => w.Name == fileName).Any())
        //    {
        //        RetrieveFTPFile(_job.GetUri().AbsoluteUri);
        //    }
        //    else
        //    {
        //        _job.JobLoggerMessage("Info", $"Remote file does not exist - FileName:{fileName}");
        //        throw new FileNotFoundException($"Remote file does not exist - FileName:{fileName}");
        //    }
        //}

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
                RetrieveFTPFile(remoteUrl);
            }
        }

        private void RetrieveFTPFile(string absoluteUri)
        {
            //Setup temporary work space for job
            var tempFile = SetupTempWorkSpace(absoluteUri);

            if (_job.JobOptions != null && _job.JobOptions.CompressionOptions.IsCompressed)
            {
                _job.JobLoggerMessage("Debug", $"Compressed option is detected... Streaming to temp location");
                _job.JobLoggerMessage("Debug", $"retrieveftpfile absoluteuri:{absoluteUri}");

                try
                {
                    //Stream file to work location
                    using (Stream ftpstream = _ftpProvider.GetFileStream(absoluteUri))
                    {
                        using (Stream filestream = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            ftpstream.CopyTo(filestream);
                        }
                    }

                    //check if target directory contains file
                    var parentDir = Directory.GetParent(tempFile);
                    var fcount = Directory.GetFiles(parentDir.FullName, "*", SearchOption.TopDirectoryOnly).Length;

                    if (fcount == 0)
                    {
                        _job.JobLoggerMessage("Warn", $"RetrieveFTPFile targetfileextractcount:{fcount.ToString()}");
                        throw new FileNotFoundException("File not found in temp file target <retrieveftpfile>");
                    }
                    else
                    {
                        _job.JobLoggerMessage("Debug", $"retrieveftpfile detectedfilesintargetdir targetdir:{parentDir.FullName} filecount:{fcount}");
                        var files = parentDir.EnumerateFiles();
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Files detected in {parentDir.FullName}");
                        foreach(var f in files)
                        {
                            sb.Append($"{f.FullName}\t{f.Length}");
                        }
                        _job.JobLoggerMessage("Debug", sb.ToString());
                    }

                    //Create a fire-forget Hangfire job to decompress the file and drop extracted file into drop locations
                    BackgroundJob.Enqueue<JawsService>(x => x.UncompressRetrieverJob(_job.Id, tempFile));
                }
                catch (Exception ex)
                {
                    _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                    _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                    //Cleanup target file if exists
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }

                    throw;
                }
            }
            //Source file is not compressed, stream to drop path location.
            else
            {
                //Find appropriate drop location (S3Basic or DfsBasic)
                RetrieverJob targetJob = FindBasicJob();

                //Get target path based on basic job found
                string targetFullPath = GetTargetPath(targetJob);

                if (targetJob.DataSource.Is<S3Basic>())
                {
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

                    S3ServiceProvider s3Service = new S3ServiceProvider();
                    string targetkey = $"{targetJob.DataSource.GetDropPrefix(targetJob)}{_job.GetTargetFileName(Path.GetFileName(targetFullPath))}";
                    var versionId = s3Service.UploadDataFile(tempFile, targetkey);

                    _job.JobLoggerMessage("Info", $"File uploaded to S3 Drop Location  (Key:{targetkey} | VersionId:{versionId})");
                }
                else if (targetJob.DataSource.Is<DfsBasic>())
                {
                    _job.JobLoggerMessage("Info", "Sending file to DFS drop location");
                    try
                    {
                        using (Stream ftpstream = _ftpProvider.GetFileStream(absoluteUri))
                        {
                            using (Stream filestream = new FileStream(targetFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                            {
                                ftpstream.CopyTo(filestream);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _job.JobLoggerMessage("Error", "Retriever job failed streaming external file.", ex);
                        _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                        //Cleanup target file if exists
                        if (File.Exists(targetFullPath))
                        {
                            File.Delete(targetFullPath);
                        }

                        throw;
                    }
                }

                //Short-Term establishes a new connection to the source and sends file to current files location
                if (_job.JobOptions.CreateCurrentFile)
                {
                    string targetFullpath = Path.Combine(_job.DatasetConfig.GetCurrentFileDir().LocalPath, _job.GetTargetFileName(Path.GetFileName(_job.GetUri().ToString())));

                    try
                    {
                        //using (Stream ftpstream = _ftpProvider.GetJobStream(_job))
                        //{
                        //    //Using FileMode.Create will overwrite file if exists
                        //    using (Stream Currentfilestream = new FileStream(targetFullpath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                        //    {
                        //        ftpstream.CopyTo(Currentfilestream);
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        _job.JobLoggerMessage("Error", "Retriever Job failed creating Current File for SAS.", ex);
                        _job.JobLoggerMessage("Info", "Performing FTP post-failure cleanup.");

                        //Clean up target file if exists
                        if (File.Exists(targetFullpath))
                        {
                            File.Delete(targetFullpath);
                        }
                    }
                }
            }
        }
        #endregion


        public async Task UpdateJobStatesAsync()
        {

            try
            {
                var jobList = _datasetContext.JobHistory.Where(w => w.Active && w.BatchId != 0).ToList();

                List<Task<HttpResponseMessage>> taskList = new List<Task<HttpResponseMessage>>();

                foreach(var job in jobList)
                {
                    taskList.Add(_jobService.GetApacheLibyBatchStatusAsync(job));
                }                

                var results = await Task.WhenAll(taskList);
            }
            catch (Exception ex)
            {
                Logger.Fatal("Unable to update job state.", ex);
            }
        }

        private string GetTargetPath(RetrieverJob basicJob)
        {
            string basepath;
            string filename;
            string targetPath = null;
            if (basicJob.DataSource.Is<DfsBasic>())
            {
                basepath = basicJob.GetUri().LocalPath + '\\';                
            }
            else if (basicJob.DataSource.Is<S3Basic>())
            {                
                basepath = basicJob.DataSource.GetDropPrefix(basicJob);
            }
            else
            {
                _job.JobLoggerMessage("Error", "Not Configured to determine target path for data source type");
                throw new NotImplementedException("Not Configured to determine target path for data source type");
            }

            if (_job.DataSource.Is<HTTPSSource>())
            {
                filename = _job.GetTargetFileName(_job.GetTargetFileName(String.Empty));
                targetPath = $"{basepath}{_job.GetTargetFileName(Path.GetFileName(_job.GetUri().ToString()))}";
            }
            else if (_job.DataSource.Is<DfsBasicHsz>())
            {
                targetPath = basepath;
            }
            else
            {
                filename = _job.GetTargetFileName(Path.GetFileName(_job.GetUri().ToString()));
                targetPath = Path.Combine(basepath, _job.GetTargetFileName(Path.GetFileName(_job.GetUri().ToString())));
            }            

            return targetPath;
        }

        private RetrieverJob FindBasicJob()
        {
            RetrieverJob basicJob = null;

            basicJob = _requestContext.RetrieverJob.Where(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && w.DataSource is S3Basic).FetchAllConfiguration(_requestContext).SingleOrDefault();

            if (basicJob == null)
            {
                _job.JobLoggerMessage("Info", "No S3Basic job found for Schema... Finding DfsBasic job");
                basicJob = _requestContext.RetrieverJob.Where(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && w.DataSource is DfsBasic).FetchAllConfiguration(_requestContext).SingleOrDefault();

                if (basicJob == null)
                {
                    _job.JobLoggerMessage("Fatal", "Failed to find basic job");
                    throw new NotImplementedException("Failed to find generic Basic job");
                }
            }
            
            return basicJob;
        }

        /// <summary>
        /// Generates temp work location and removes temp file if already exists
        /// </summary>
        /// <returns></returns>
        public string SetupTempWorkSpace(string fileName = null)
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

        private void ProcessCompressedFile(string filepath, bool deleteSrcFile)
        {
            //This job will process both the incoming file and the uncompressed file produced from jaws 
            //  since jaws will drop the output within the appropriate drop location\
            //With this said, we need to detect if the incoming file is compressed first

            var compression = (CompressionTypes)Enum.Parse(typeof(CompressionTypes), _job.JobOptions.CompressionOptions.CompressionType);

            //CompressionTypes extension method used to determine if incoming file matches compression type job is configured for
            if (compression.FileMatchCompression(filepath))
            {
                //... process compressed file
                _job.JobLoggerMessage("Info", $"Compressed option is detected... Streaming to temp location");

                //var tempFile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Path.GetFileName(filepath));

                var tempFile = SetupTempWorkSpace(filepath);

                //TODO: Revisit delete source file logic to handle not deleting source file
                if (deleteSrcFile)
                {
                    
                    if (_job.DataSource.Is<DfsBasic>() || _job.DataSource.Is<DfsCustom>())
                    {
                        //Rename incoming file (prefix with !!) so future jobs do not pick it up.
                        //1.) Get path
                        var orginalPath = filepath.Replace(Path.GetFileName(filepath), "");
                        //2.) Pull out just file name
                        var origFileName = Path.GetFileName(filepath);
                        //3.) Prefix filename with ProcessedFilePrefix (!!).
                        var processingFile = orginalPath + Configuration.Config.GetHostSetting("ProcessedFilePrefix") + origFileName;

                        //Rename file to indicate a request is being processed
                        File.Move(filepath, processingFile);

                        ////Create temp directory if exists
                        //Directory.CreateDirectory(Path.GetDirectoryName(tempFile));

                        //Stream file to work location
                        using (Stream incomingfs = new FileStream(processingFile, FileMode.Open, FileAccess.Read))
                        {
                            using (Stream newfs = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                incomingfs.CopyTo(newfs);
                            }
                        }


                        //check if target directory contains file
                        var parentDir = Directory.GetParent(tempFile);
                        var fcount = Directory.GetFiles(parentDir.FullName, "*", SearchOption.TopDirectoryOnly).Length;

                        if (fcount == 0)
                        {
                            _job.JobLoggerMessage("Warn", $"processcompressedfile targetfileextractcount:{fcount.ToString()}");
                            throw new FileNotFoundException("File not found in temp file target <processcompressedfile>");
                        }
                        else
                        {
                            _job.JobLoggerMessage("Debug", $"processcompressedfile detectedfilesintargetdir targetdir:{parentDir.FullName} filecount:{fcount}");
                            var files = parentDir.EnumerateFiles();
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"Files detected in {parentDir.FullName}");
                            foreach (var f in files)
                            {
                                sb.Append($"{f.FullName}\t{f.Length}");
                            }
                            _job.JobLoggerMessage("Debug", sb.ToString());
                        }

                        //Delete file within drop location
                        try
                        {
                            File.Delete(processingFile);
                        }
                        catch (Exception ex)
                        {
                            _job.JobLoggerMessage("Error", $"Failed Deleting File from drop location : ({filepath})", ex);
                        }
                    }
                    else if (_job.DataSource.Is<S3Basic>())
                    {
                        S3ServiceProvider s3Service = new S3ServiceProvider();

                        //Create temp directory if exists
                        Directory.CreateDirectory(Path.GetDirectoryName(tempFile));

                        //Stream file to work location
                        using (Stream incomingfs = s3Service.GetObject(filepath))
                        {
                            using (Stream newfs = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                incomingfs.CopyTo(newfs);
                            }
                        }

                        ObjectKeyVersion deleteobject = s3Service.MarkDeleted(filepath);
                        _job.JobLoggerMessage("Info", $"Deleted S3 Drop Location Object (Delete Object key:{deleteobject.key} versionid:{deleteobject.versionId})");                        
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                //Create a fire-forget Hangfire job to decompress the file and drop extracted file into drop location
                BackgroundJob.Enqueue<JawsService>(x => x.UncompressRetrieverJob(_job.Id, tempFile));

                //If a retriever job should be kicked off as soon as jaws if finished, Pass the ID from the enqueued job above to the code below.
                //BackgroundJob.ContinueWith<RetrieverJobService>(id, RetrieverJobService => RetrieverJobService.RunRetrieverJob(_job.Id), JobContinuationOptions.OnlyOnSucceededState);
            }
            else
            {
                //TODO: https://jira.sentry.com/browse/DSC-865 : Limit what file extentions can be processed.
                //Submit Loader Request
                SubmitLoaderRequest(filepath);
            }
        }

        private string GenerateProcessingFileName(string filepath)
        {
            var orginalPath = Path.GetFullPath(filepath).Replace(Path.GetFileName(filepath), "");
            var origFileName = Path.GetFileName(filepath);
            return orginalPath + Configuration.Config.GetHostSetting("ProcessedFilePrefix") + origFileName;
        }

        private string GetFileOwner(string filepath)
        {
            var fsecurity = File.GetAccessControl(filepath);
            var sid = fsecurity.GetOwner(typeof(SecurityIdentifier));
            var ntAccount = sid.Translate(typeof(NTAccount));

            //remove domain
            return ntAccount.ToString().Replace(@"SHOESD01\", "");
        }

        // TODO - CLA-2372 - REMOVE COMMENTED CODE - Removing DFSBasic, DfsCustom, S3Basic references
        private void SubmitLoaderRequest(string filepath)
        {
            string processingFile = null;
            string fileOwner = null;

            if (_job.DataSource.Is<DfsBasic>() || _job.DataSource.Is<DfsCustom>())
            {
                processingFile = GenerateProcessingFileName(filepath);
                fileOwner = GetFileOwner(filepath);

                //Rename file to indicate a request has been sent to Dataset Loader
                File.Move(filepath, processingFile);
            }
            else if (_job.DataSource.Is<S3Basic>())
            {
                processingFile = filepath;
                fileOwner = Configuration.Config.GetHostSetting("ServiceAccountID");
            }
            else
            {
                throw new NotImplementedException("Retriever service not configured for DataSource Type");
            }

            //Create Dataset Loader request
            var hashInput = $"{Configuration.Config.GetHostSetting("ServiceAccountID")}_{DateTime.Now.ToString("MM-dd-yyyyHH:mm:ss.fffffff")}_{filepath}";

            LoaderRequest loadReq = new LoaderRequest(GenerateHash(hashInput));
            loadReq.File = processingFile;
            loadReq.IsBundled = false;
            loadReq.DatasetID = _job.DatasetConfig.ParentDataset.DatasetId;
            loadReq.DatasetFileConfigId = _job.DatasetConfig.ConfigId;
            loadReq.RetrieverJobId = _job.Id;
            loadReq.RequestInitiatorId = fileOwner;

            string jsonReq = JsonConvert.SerializeObject(loadReq, Formatting.Indented);

            //Send request to DFS location loader service is watching for requests
            using (MemoryStream ms = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(ms);

                writer.WriteLine(jsonReq);
                writer.Flush();

                //You have to rewind the MemoryStream before copying
                ms.Seek(0, SeekOrigin.Begin);

                using (FileStream fs = new FileStream($"{Sentry.Configuration.Config.GetHostSetting("LoaderRequestPath")}{loadReq.RequestGuid}.json", FileMode.OpenOrCreate))
                {
                    ms.CopyTo(fs);
                    fs.Flush();
                }                
            }
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }

            return false;
        }

        /// <summary>
        /// Generates hash based on input, returns GUID 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private Guid GenerateHash(string input)
        {
            Guid result;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                result = new Guid(hash);
            }
            return result;
        }
        
    }
}

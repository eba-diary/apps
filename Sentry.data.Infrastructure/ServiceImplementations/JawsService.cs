using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using StructureMap;
using Sentry.Common.Logging;
using Hangfire;
using System.IO;
using System.IO.Compression;

namespace Sentry.data.Infrastructure
{
    [Queue("jawsservice")]
    public class JawsService
    {
        private RetrieverJob _job;

        private IContainer Container { get; set; }

        public void UncompressRetrieverJob(int jobId, string filePath)
        {
            Logger.Info($"Starting JawsService for Job Id : {jobId}");

            using (Container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
            {
                IRequestContext _requestContext = Container.GetInstance<IRequestContext>();

                _job = _requestContext.RetrieverJob.FirstOrDefault(w => w.Id == jobId);

                _job.JobLoggerMessage("Info", "Jaws processing triggered");

                //This can be triggered by various jobs, we need to pick the drop location to move the uncompressed files too.
                // Since GoleneEye will be running on AWS and Jaw decompression takes place on a local directory of the EC2 instance,
                // we would prefer to drop the file in the S3 drop location to minimize up\down traffic from AWS.  
                //Therefore, we search for the generic jobs associated with the datasetConfig of the current job.  Then pick out the
                // DFS and S3 basic jobs.  If S3Basic job exists, upload to S3 drop location, otherwise, copy file to DFS drop location.  
                List<RetrieverJob> genericJobs = _requestContext.RetrieverJob.Where(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && (w.DataSource is DfsBasic || w.DataSource is S3Basic)).ToList();
                RetrieverJob defaultjob = null;

                defaultjob = genericJobs.FirstOrDefault(w => w.DataSource.Is<S3Basic>());
                if (defaultjob == null)
                {
                    _job.JobLoggerMessage("Info", "S3Basic job not found, finding DfsBasic job");
                    defaultjob = genericJobs.FirstOrDefault(w => w.DataSource.Is<DfsBasic>());
                    if (defaultjob == null)
                    {
                        _job.JobLoggerMessage("Error", "No Default jobs were found");
                        throw new NotImplementedException("No Default jobs (DfsBasic or S3Basic) where found for schema");
                    }
                }
                else
                {
                    _job.JobLoggerMessage("Info", "S3Basic job found");                    
                }

                
                //Check for .Zip extension
                if (Convert.ToInt32(_job.JobOptions.CompressionOptions.CompressionType) == (int)CompressionTypes.ZIP)
                {
                    try
                    {
                        //Process only if file matches file search criteria
                        if (!_job.FilterIncomingFile(Path.GetFileName(filePath)))
                        {
                            if (defaultjob.DataSource.Is<S3Basic>())
                            {
                                S3ServiceProvider s3Service = new S3ServiceProvider();

                                //local directory where compressed file contents will be extracted
                                var extractPath = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Path.GetFileNameWithoutExtension(filePath));

                                //create local extraction directory
                                if (!Directory.Exists(extractPath))
                                {
                                    Directory.CreateDirectory(extractPath);
                                }

                                try
                                {
                                    //Do we need to exclude any files from zip archive?  If not extract all files to target directory
                                    if (_job.JobOptions.CompressionOptions.FileNameExclusionList != null && _job.JobOptions.CompressionOptions.FileNameExclusionList.Count > 0)
                                    {
                                        List<String> ExclusionList = _job.JobOptions.CompressionOptions.FileNameExclusionList;
                                        
                                        using (ZipArchive archive = ZipFile.OpenRead(filePath))
                                        {
                                            foreach (ZipArchiveEntry entry in archive.Entries)
                                            {
                                                //Exclude file if name exists in ExclusionList or does not match job search criteria
                                                if (!ExclusionList.Contains(entry.FullName) && !_job.FilterIncomingFile(entry.FullName))
                                                {
                                                    //extract to local work directory, overrwrite file if exists
                                                    entry.ExtractToFile(Path.Combine(extractPath, entry.FullName), true);
                                                }
                                            }
                                        }

                                        //Upload all extracted files to S3 drop location
                                        foreach (string file in Directory.GetFiles(extractPath))
                                        {
                                            string targetkey = $"{defaultjob.DataSource.GetDropPrefix(defaultjob)}{_job.GetTargetFileName(Path.GetFileNameWithoutExtension(file))}";
                                            var versionId = s3Service.UploadDataFile(file, targetkey);
                                            _job.JobLoggerMessage("Info", $"Extracted File to S3 Drop Location (key:{targetkey} | versionId:{versionId})");
                                        }                                                                        
                                    }
                                    else
                                    {
                                        ZipFile.ExtractToDirectory(filePath, extractPath);

                                        //Upload all extracted files to S3 drop location
                                        foreach (string file in Directory.GetFiles(extractPath))
                                        {
                                            string targetkey = $"{defaultjob.DataSource.GetDropPrefix(defaultjob)}{_job.GetTargetFileName(Path.GetFileNameWithoutExtension(file))}";
                                            var versionId = s3Service.UploadDataFile(file, targetkey);
                                            _job.JobLoggerMessage("Info", $"Extracted File contents to S3 Drop Location (key:{targetkey} | versionId:{versionId})");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _job.JobLoggerMessage("Error", "Jaws failed extracting and uploading to S3 drop location", ex);
                                }
                                finally
                                {
                                    _job.JobLoggerMessage("Info", $"Deleting all files in extract directory ({extractPath})");

                                    //cleanup local extracts
                                    CleanupTempDir(extractPath);
                                }

                                //cleanup Source temp file after successful processing
                                CleanupTempFile(filePath);
                            }
                            else if (defaultjob.DataSource.Is<DfsBasic>())
                            {
                                //Set target path based on DfsBasic URI
                                string targetPath = defaultjob.GetUri().LocalPath;

                                if (_job.JobOptions.CompressionOptions.FileNameExclusionList != null && _job.JobOptions.CompressionOptions.FileNameExclusionList.Count > 0)
                                {
                                    List<String> ExclusionList = _job.JobOptions.CompressionOptions.FileNameExclusionList;

                                    using (ZipArchive archive = ZipFile.OpenRead(filePath))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            //Exclude file if name exists in ExclusionList or does not match job search criteria
                                            if (!ExclusionList.Contains(entry.FullName) || !_job.FilterIncomingFile(entry.FullName))
                                            {
                                                //extract to DfsBasic drop location
                                                entry.ExtractToFile(Path.Combine(targetPath, entry.FullName));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ZipFile.ExtractToDirectory(filePath, targetPath);
                                }

                                //cleanup temp file after successful processing
                                CleanupTempFile(filePath);
                            }
                        }
                    }
                    catch(System.IO.InvalidDataException ex)
                    {
                        _job.JobLoggerMessage("Error", $"Jaws could not decompress file ({Path.GetFileName(filePath)}), 1) corrupt file, 2) is not .zip archive, or 3) there is more than 1 part to the archive (multi part zip).", ex);
                        
                        //throw exeption to ensure HangFire job is failed
                        throw new InvalidDataException($"Jaws could not decompress job ({_job.Id}) file ({Path.GetFileName(filePath)}), 1) corrupt file, 2) is not .zip archive, or 3) there is more than 1 part to the archive.", ex);
                    }
                    catch(Exception ex)
                    {
                        //throw exeption to ensure HangFire job is failed
                        _job.JobLoggerMessage("Error", $"Jaw could not decompress file ({Path.GetFileName(filePath)})", ex);
                    }                    
                }
                //Check for gzip extension
                else if (Convert.ToInt32(_job.JobOptions.CompressionOptions.CompressionType) == (int)CompressionTypes.GZIP)
                {
                    string tempfile = null;

                    try
                    {                        
                        //Process only if file matches file search criteria
                        if (!_job.FilterIncomingFile(Path.GetFileNameWithoutExtension(filePath)))
                        {
                            //Extract files within local work directory
                            tempfile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Path.GetFileNameWithoutExtension(filePath));

                            //remove temp file if already exists
                            if (File.Exists(tempfile)) {
                                File.Delete(tempfile);
                            }

                            using (Stream fd = File.Create(tempfile))
                            using (Stream fs = File.OpenRead(filePath))
                            using (Stream csStream = new GZipStream(fs, CompressionMode.Decompress))
                            {
                                byte[] buffer = new byte[1024];
                                int nRead;
                                while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fd.Write(buffer, 0, nRead);
                                }
                            }                            
                                
                            if(defaultjob.DataSource.Is<S3Basic>())
                            {
                                S3ServiceProvider s3Service = new S3ServiceProvider();                                
                                string targetkey = $"{defaultjob.DataSource.GetDropPrefix(defaultjob)}{_job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath))}";
                                var versionId = s3Service.UploadDataFile(tempfile, targetkey);
                                _job.JobLoggerMessage("Info", $"Extracted File to S3 Drop Location (Key:{targetkey} VersionId:{versionId}");
                            }
                            else if(defaultjob.DataSource.Is<DfsBasic>())
                            {                                    
                                string target = Path.Combine(defaultjob.GetUri().LocalPath, _job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath)));
                                File.Move(tempfile, target);
                                _job.JobLoggerMessage("Info", $"Extracted File to DFS Drop Location ({target})");
                            }
                            else
                            {
                                _job.JobLoggerMessage("Error", $"The Schema does not have DFS or S3 basic job defined");
                                throw new NotImplementedException($"The Dataset config ({_job.DatasetConfig.ConfigId}) does not have a generic DFS or S3 basic job defined.");
                            }                           
                        }
                        else
                        {
                            _job.JobLoggerMessage("Warn", $"Incoming file filtered ({filePath})");
                            _job.JobLoggerMessage("Info", $"Cleaning up incoming file ({filePath})");
                            CleanupTempFile(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _job.JobLoggerMessage("Error", $"Jaws could not decompress file ({Path.GetFileName(filePath)})", ex);

                        //throw exeption to ensure HangFire job is failed
                        throw new InvalidDataException($"Jaws could not decompress job ({_job.Id}) file ({Path.GetFileName(filePath)}).", ex);
                    }
                    finally
                    {
                        //cleanup extracted files from work directory
                        try
                        {
                            _job.JobLoggerMessage("Info", $"Cleaning up extracted files");
                            File.Delete(tempfile);
                        }
                        catch (Exception ex)
                        {
                            // Log error but allow process to continue successfully
                            _job.JobLoggerMessage("Error", $"Failed Deleting Extracted file from temp directory", ex);
                        }
                    }

                    //cleanup temp compressed file after successful processing
                    CleanupTempFile(filePath);
                }
                //Extension does not match configured compression logic
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void CleanupTempFile(string filePath)
        {
            try
            {
                _job.JobLoggerMessage("Info", $"Cleaning up job temp file ({filePath})");
                File.Delete(filePath);                
            }
            catch (Exception ex)
            {
                // Log error but allow process to continue successfully
                _job.JobLoggerMessage("Warn", $"Failed Deleting Job Temp file ({filePath})", ex);
            }
        }

        private void CleanupTempDir(string dirPath)
        {
            try
            {
                _job.JobLoggerMessage("Info", $"Cleaning up Job temp directory ({dirPath})");
                Directory.Delete(dirPath,true);
            }
            catch (Exception ex)
            {
                // Log error but allow process to continue successfully
                _job.JobLoggerMessage("Warn", $"Failed Deleting Job Temp Directory ({dirPath})", ex);
            }
        }
    }
}


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

                _job = _requestContext.RetrieverJob.Where(w => w.Id == jobId).FirstOrDefault();

                //Check for .Zip extension
                if (Convert.ToInt32(_job.JobOptions.CompressionOptions.CompressionType) == (int)CompressionTypes.ZIP)
                {
                    try
                    {
                        //Do we need to exclude any files from zip archive?  If not extract all files to target directory
                        if (_job.JobOptions.CompressionOptions.FileNameExclusionList != null && _job.JobOptions.CompressionOptions.FileNameExclusionList.Count > 0)
                        {
                            List<String> ExclusionList = _job.JobOptions.CompressionOptions.FileNameExclusionList;

                            using (ZipArchive archive = ZipFile.OpenRead(filePath))
                            {
                                foreach(ZipArchiveEntry entry in archive.Entries)
                                {
                                    //Exclude file if name exists in ExclusionList or does not match job search criteria
                                    if (!ExclusionList.Contains(entry.FullName) || !_job.FilterIncomingFile(entry.FullName))
                                    {
                                        entry.ExtractToFile(Path.Combine(_job.GetUri().LocalPath, _job.GetTargetFileName(entry.FullName)));
                                    }
                                }
                            }
                        }
                        else
                        {
                            ZipFile.ExtractToDirectory(filePath, _job.GetUri().LocalPath);
                        }

                        //cleanup temp file after successful processing
                        CleanupTempFile(filePath);
                    }
                    catch(System.IO.InvalidDataException ex)
                    {
                        Logger.Error($"Jaws could not decompress job ({_job.Id}) file ({Path.GetFileName(filePath)}), 1) corrupt file, 2) is not .zip archive, or 3) there is more than 1 part to the archive (multi part zip).", ex);

                        //throw exeption to ensure HangFire job is failed
                        throw new InvalidDataException($"Jaws could not decompress job ({_job.Id}) file ({Path.GetFileName(filePath)}), 1) corrupt file, 2) is not .zip archive, or 3) there is more than 1 part to the archive.", ex);
                    }
                    catch(Exception ex)
                    {
                        //throw exeption to ensure HangFire job is failed
                        Logger.Error($"Jaw could not decompress job ({_job.Id}) file ({Path.GetFileName(filePath)}).", ex);
                    }                    
                }
                //Check for gzip extension
                else if (Convert.ToInt32(_job.JobOptions.CompressionOptions.CompressionType) == (int)CompressionTypes.GZIP)
                {
                    try
                    {
                        //Process only if file matches file search criteria
                        if (!_job.FilterIncomingFile(Path.GetFileNameWithoutExtension(filePath)))
                        {
                            //if (_job.DataSource.Is<DfsBasic>() || _job.DataSource.Is<DfsCustom>())
                            //{
                            //    // Decompress
                            //    using (Stream fd = File.Create(Path.Combine(_job.DatasetConfig.DropPath, _job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath)))))
                            //    using (Stream fs = File.OpenRead(filePath))
                            //    using (Stream csStream = new GZipStream(fs, CompressionMode.Decompress))
                            //    {
                            //        byte[] buffer = new byte[1024];
                            //        int nRead;
                            //        while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
                            //        {
                            //            fd.Write(buffer, 0, nRead);
                            //        }
                            //    }
                            //}
                            //else if (_job.DataSource.Is<S3Basic>() || _job.DataSource.Is<FtpSource>())
                            //{
                                //Extract files within local work directory
                                string tempfile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Path.GetFileNameWithoutExtension(filePath));

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

                                //This can be triggered by various jobs, we need to pick the drop location to move the uncompressed files too.
                                // Since GoleneEye will be running on AWS and Jaw decompression takes place on a local directory of the EC2 instance,
                                // we would prefer to drop the file in the S3 drop location to minimize up\down traffic from AWS.  
                                //Therefore, we search for the generic jobs associated with the datasetConfig of the current job.  Then pick out the
                                // DFS and S3 basic jobs.  If S3Basic job exists, upload to S3 drop location, otherwise, copy file to DFS drop location.  
                                List<RetrieverJob> genericJobs = _requestContext.RetrieverJob.Where(w => w.DatasetConfig.ConfigId == _job.DatasetConfig.ConfigId && w.IsGeneric).ToList();
                                RetrieverJob defaultS3job = null;
                                RetrieverJob defaultDfsjob = null;

                                foreach (RetrieverJob job in genericJobs)
                                {
                                    if (job.DataSource.Is<S3Basic>())
                                    {
                                        defaultS3job = job;
                                    }
                                    else if (job.DataSource.Is<DfsBasic>())
                                    {
                                        defaultDfsjob = job;
                                    }
                                }
                                
                                if(defaultS3job != null)
                                {
                                    S3ServiceProvider s3Service = new S3ServiceProvider();
                                    string targetkey = $"{defaultS3job.DataSource.GetDropPrefix(defaultS3job)}{_job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath))}";
                                    var versionId = s3Service.UploadDataFile(tempfile, targetkey);
                                    Logger.Info($"Extracted File to S3 Drop Location for Job({_job.Id}) : Key:{targetkey} VersionId:{versionId}");
                                }
                                else if(defaultDfsjob != null)
                                {                                    
                                    string target = Path.Combine(defaultDfsjob.GetUri().LocalPath, _job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath)));
                                    File.Move(tempfile, target);
                                    Logger.Info($"Extracted File to DFS Drop Location for Job({_job.Id}) : Location:{target}");
                                }
                                else
                                {
                                    throw new NotImplementedException($"The Dataset config ({_job.DatasetConfig.ConfigId}) does not have a generic DFS or S3 basic job defined.");
                                }

                                //cleanup extracted files from work directory
                                try
                                {
                                    Logger.Debug($"Cleaning up extracted files for job");
                                    File.Delete(tempfile);
                                }
                                catch (Exception ex)
                                {
                                    // Log error but allow process to continue successfully
                                    Logger.Error("Failed Deleting Extracted file from temp directory", ex);
                                }
                            //}
                            //else
                            //{
                            //    throw new NotImplementedException();
                            //}                            

                            //cleanup temp compressed file after successful processing
                            CleanupTempFile(filePath);
                        }  
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Jaws could not decompress job ({_job.Id}) file ({Path.GetFileName(filePath)}).", ex);

                        //throw exeption to ensure HangFire job is failed
                        throw new InvalidDataException($"Jaws could not decompress job ({_job.Id}) file ({Path.GetFileName(filePath)}).", ex);
                    }
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
                Logger.Debug($"Cleaning up Job temp file ({filePath})");
                File.Delete(filePath);                
            }
            catch (Exception ex)
            {
                // Log error but allow process to continue successfully
                Logger.Error("Failed Deleting Job Temp Directory", ex);
            }
        }
    }
}


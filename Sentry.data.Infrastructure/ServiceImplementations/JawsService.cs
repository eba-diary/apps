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
            try
            {
                Logger.Info($"Starting JawsService for Job Id : {jobId}");

                using (Container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                {
                    IRequestContext _requestContext = Container.GetInstance<IRequestContext>();

                    _job = _requestContext.RetrieverJob.FirstOrDefault(w => w.Id == jobId);

                    _job.JobLoggerMessage("Info", "Jaws processing triggered");
                    _job.JobLoggerMessage("Info", $"uncompressretrieverjob incomingfile:{filePath}");

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
                            _job.JobLoggerMessage("Info", "uncompressretrieverjob detected_zip_compression");
                            if (defaultjob.DataSource.Is<S3Basic>())
                            {
                                S3ServiceProvider s3Service = new S3ServiceProvider();

                                //local directory where compressed file contents will be extracted
                                //Adding guid to path to ensure concurrent processing of zip file with same name
                                var extractPath = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(filePath));

                                //create local extraction directory
                                if (!Directory.Exists(extractPath))
                                {
                                    Directory.CreateDirectory(extractPath);
                                }

                                _job.JobLoggerMessage("Info", $"uncompressedretrieverjob extractpath:{extractPath}");

                                try
                                {
                                    UncompressZipFile(filePath, extractPath);

                                    string[] extractedFileList = GetAllFilesWithinDirectory(extractPath, true);
                                    LogExtractionContents(extractPath, extractedFileList);

                                    //Upload all extracted files to S3 drop location
                                    foreach (string file in extractedFileList)
                                    {
                                        _job.JobLoggerMessage("Info", $"uncompressretrieverjob preparing_file_upload file:{Path.GetFileName(file)}");

                                        //Adding parent directory to targetkey to ensure concurrent processing of extracted files with same name
                                        // an additional parent folder named with a GUID is added to ensure concurrent processing of same zip file name
                                        string targetkey = $"{defaultjob.DataSource.GetDropPrefix(defaultjob)}{Guid.NewGuid().ToString()}/{Directory.GetParent(file).Name}/{_job.GetTargetFileName(Path.GetFileName(file))}";
                                        var versionId = s3Service.UploadDataFile(file, targetkey);
                                        _job.JobLoggerMessage("Info", $"Extracted File contents to S3 Drop Location (key:{targetkey} | versionId:{versionId})");
                                    }

                                    _job.JobLoggerMessage("Info", "uncompressretrieverjob completed_extraction");
                                }
                                catch (Exception ex)
                                {
                                    _job.JobLoggerMessage("Error", "Jaws failed extracting and uploading to S3 drop location", ex);
                                }
                                finally
                                {
                                    _job.JobLoggerMessage("Info", $"Deleting all files in extract directory ({extractPath})");

                                    //cleanup local extracts
                                    //  Need to send the parent directory since we generate an additional layer, to support
                                    //  concurrency, containing a unique GUID as the folder name
                                    CleanupTempDir(Directory.GetParent(extractPath).FullName);
                                }

                                //cleanup Source temp file after successful processing
                                CleanupTempDir(Directory.GetParent(filePath).FullName);
                            }
                            else if (defaultjob.DataSource.Is<DfsBasic>())
                            {
                                //Set target path based on DfsBasic URI
                                string targetPath = ""; //defaultjob.GetUri().LocalPath; Replaced with some junk

                                UncompressZipFile(filePath, targetPath);

                                string[] extractedFileList = GetAllFilesWithinDirectory(targetPath, true);
                                LogExtractionContents(targetPath, extractedFileList);

                                //cleanup temp file after successful processing
                                CleanupTempDir(Directory.GetParent(filePath).FullName);
                            }
                        }
                        catch (System.IO.InvalidDataException ex)
                        {
                            _job.JobLoggerMessage("Error", $"Jaws could not decompress file ({Path.GetFileName(filePath)}), 1) corrupt file, 2) is not .zip archive, or 3) there is more than 1 part to the archive (multi part zip).", ex);

                            //throw exeption to ensure HangFire job is failed
                            throw new InvalidDataException($"Jaws could not decompress job ({_job.Id}) file ({Path.GetFileName(filePath)}), 1) corrupt file, 2) is not .zip archive, or 3) there is more than 1 part to the archive.", ex);
                        }
                        catch (Exception ex)
                        {
                            //throw exeption to ensure HangFire job is failed
                            _job.JobLoggerMessage("Error", $"Jaw could not decompress file ({Path.GetFileName(filePath)})", ex);
                        }
                    }
                    //Check for gzip extension
                    else if (Convert.ToInt32(_job.JobOptions.CompressionOptions.CompressionType) == (int)CompressionTypes.GZIP)
                    {
                        _job.JobLoggerMessage("Info", "uncompressretrieverjob detected_gzip_compression");
                        string tempfile = null;

                        try
                        {
                            //Extract files within local work directory
                            tempfile = Path.Combine(Configuration.Config.GetHostSetting("GoldenEyeWorkDir"), "Jobs", _job.Id.ToString(), Path.GetFileNameWithoutExtension(filePath));

                            //remove temp file if already exists
                            if (File.Exists(tempfile))
                            {
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

                            if (defaultjob.DataSource.Is<S3Basic>())
                            {
                                S3ServiceProvider s3Service = new S3ServiceProvider();
                                string targetkey = $"{defaultjob.DataSource.GetDropPrefix(defaultjob)}{_job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath))}";
                                var versionId = s3Service.UploadDataFile(tempfile, targetkey);
                                _job.JobLoggerMessage("Info", $"Extracted File to S3 Drop Location (Key:{targetkey} VersionId:{versionId}");
                            }
                            else if (defaultjob.DataSource.Is<DfsBasic>())
                            {
                                string target = ""; //Replaced with some junk Path.Combine(defaultjob.GetUri().LocalPath, _job.GetTargetFileName(Path.GetFileNameWithoutExtension(filePath)));
                                File.Move(tempfile, target);
                                _job.JobLoggerMessage("Info", $"Extracted File to DFS Drop Location ({target})");
                            }
                            else
                            {
                                _job.JobLoggerMessage("Error", $"The Schema does not have DFS or S3 basic job defined");
                                throw new NotImplementedException($"The Dataset config ({_job.DatasetConfig.ConfigId}) does not have a generic DFS or S3 basic job defined.");
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
                        CleanupTempDir(Directory.GetParent(filePath).FullName);
                    }
                    //Extension does not match configured compression logic
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                Logger.Info($"Completed JawsService for Job Id : {jobId}");
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Error", "uncompressretrieverjob failed", ex);
            }            
        }

        private void LogExtractionContents(string extractPath, string[] extractedFileList)
        {
            int fcount = extractedFileList.Length;
            int dcount = Directory.GetDirectories(extractPath, "*", SearchOption.AllDirectories).Length;

            if (fcount == 0)
            {
                _job.JobLoggerMessage("Warn", $"uncompressretrieverjob uncompressed_file_count:{fcount.ToString()} extractpath:{extractPath}");
            }
            else
            {
                _job.JobLoggerMessage("Debug", $"uncompressretrieverjob uncompressed_file_count:{fcount.ToString()} extractpath:{extractPath}");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"uncompressretrieverjob uncompress_dir_content: {extractPath}");
                foreach (string f in extractedFileList)
                {
                    FileInfo fi = new FileInfo(f);
                    sb.AppendLine($"name:{f}\tsize:{fi.Length}");
                }
                _job.JobLoggerMessage("Debug", sb.ToString());
            }
        }

        private void UncompressZipFile(string filePath, string extractPath)
        {
            try
            {
                _job.JobLoggerMessage("Info", "uncompresszipfile starting_extraction");
                //Do we need to exclude any files from zip archive?  If not extract all files to target directory
                if (_job.JobOptions.CompressionOptions.FileNameExclusionList != null && _job.JobOptions.CompressionOptions.FileNameExclusionList.Count > 0)
                {

                    List<string> ExclusionList = _job.JobOptions.CompressionOptions.FileNameExclusionList;

                    _job.JobLoggerMessage("Info", $"uncompresszipfile filenameexclusions_detected  count:{ExclusionList.Count}");

                    using (ZipArchive archive = ZipFile.OpenRead(filePath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            //Exclude file if name exists in ExclusionList or does not match job search criteria
                            if (!ListContainsValue(ExclusionList, Path.GetFileName(entry.FullName)) && !_job.FilterIncomingFile(entry.FullName))
                            {
                                //extract to local work directory, overrwrite file if exists
                                _job.JobLoggerMessage("Debug", $"uncompresszipfile extractfile entry:{entry.FullName} to:{Path.Combine(extractPath, Path.GetFileName(entry.FullName))}");
                                entry.ExtractToFile(Path.Combine(extractPath, Path.GetFileName(entry.FullName)), true);
                            }
                        }
                    }
                }
                else
                {
                    _job.JobLoggerMessage("Debug", $"uncompresszipfile extractdirectory source:{filePath} to:{extractPath}");
                    ZipFile.ExtractToDirectory(filePath, extractPath);
                }
            }
            catch (Exception ex)
            {
                _job.JobLoggerMessage("Debug", $"uncompresszipfile failedextraction source:{filePath} to:{extractPath}", ex);
            }            
        }

        private static bool ListContainsValue(List<string> list, string value)
        {
            bool v = list.Any(w => w.Contains(value));
            return v;
        } 

        private static string[] GetAllFilesWithinDirectory(string extractPath, bool recursively)
        {
            if (recursively)
            {
                return Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories);
            }
            else
            {
                return Directory.GetFiles(extractPath, "*", SearchOption.TopDirectoryOnly);
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


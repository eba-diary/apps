using Amazon.S3;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using Sentry.Configuration;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sentry.data.Common
{
    /// <summary>
    /// Provides common code between projects
    /// </summary>
    /// 
    public static class Utilities
    {
        private static string _bucket;
        private static string RootBucket
        {
            get
            {
                if (_bucket == null)
                {
                    _bucket = Config.GetHostSetting("AWS2_0RootBucket");
                }
                return _bucket;
            }
        }


        /// <summary>
        /// Generates full drop location path for a dataset
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string GenerateDatasetDropLocation(Dataset ds)
        {
            string filep = Path.Combine(Configuration.Config.GetHostSetting("DatasetLoaderBaseLocation"), ds.DatasetCategories.First().Name.ToLower());
            filep = Path.Combine(filep, ds.DatasetName.Replace(' ', '_').ToLower());
            return filep.ToString();
        }
        /// <summary>
        /// Generates full drop location path for a dataset.  <i>Specify all parameters</i>
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static string GenerateDatasetDropLocation(string categoryName, string datasetName)
        {
            string filep = Path.Combine(Configuration.Config.GetHostSetting("DatasetLoaderBaseLocation"), categoryName.ToLower());
            filep = Path.Combine(filep, datasetName.Replace(' ', '_').ToLower()) + @"\";
            return filep.ToString();
        }

        /// <summary>
        /// Generate storage location path.  <i>Specify all parameters</i>
        /// </summary>
        /// <param name="levels"></param>
        /// <returns></returns>
        public static string GenerateCustomStorageLocation(string[] levels)
        {
            StringBuilder result = new StringBuilder();
            result.Append(Configuration.Config.GetHostSetting("S3DataPrefix"));
            foreach (string level in levels)
            {
                result.Append(level);
                result.Append('/');
            }
            return result.ToString();
        }
        /// <summary>
        /// Generate storage key for datafile
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="now"></param>
        /// <param name="filename"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string GenerateDatafileKey(Dataset ds, DateTime now, string filename, DatasetFileConfig config)
        {
            StringBuilder location = new StringBuilder();
            location.Append(GenerateLocationKey(config));
            location.Append(now.Year.ToString());
            location.Append('/');
            location.Append(now.Month.ToString());
            location.Append('/');
            location.Append(now.Day.ToString());
            location.Append('/');
            location.Append(filename);

            return location.ToString();
        }

        /// <summary>
        /// Returns storage path
        /// </summary>
        /// <param name="datasetFileConfig"></param>
        /// <returns></returns>
        public static string GenerateLocationKey(DatasetFileConfig datasetFileConfig)
        {
            StringBuilder location = new StringBuilder();
            location.Append(Configuration.Config.GetHostSetting("S3DataPrefix"));
            location.Append(datasetFileConfig.GetStorageCode());
            location.Append('/');

            return location.ToString();
        }
        /// <summary>
        /// Generates directory friendly dataset name
        /// </summary>
        /// <param name="dsName"></param>
        /// <returns></returns>
        public static string FormatDatasetName(string dsName)
        {
            string name = null;

            name = dsName.ToLower();
            name = name.Replace(' ', '_');

            return name;
        }

        /// <summary>
        /// Generates abbreviated frequency name
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static string GenerateDatasetFrequencyLocationName(string frequency)
        {
            string freq = null;
            switch (frequency.ToLower())
            {
                case "yearly":
                    freq = "yrly";
                    break;
                case "quarterly":
                    freq = "qrtly";
                    break;
                case "monthly":
                    freq = "mntly";
                    break;
                case "weekly":
                    freq = "wkly";
                    break;
                case "daily":
                    freq = "dly";
                    break;
                case "nonschedule":
                    freq = "nskd";
                    break;
                case "transaction":
                    freq = "trn";
                    break;
                default:
                    freq = "dflt";
                    break;
            };
            return freq;
        }
        /// <summary>
        /// Return distinct list of file extensions within a list of datasetfile objects
        /// </summary>
        /// <param name="dfList"></param>
        /// <returns></returns>
        public static List<string> GetDistinctFileExtensions(IList<DatasetFile> dfList)
        {
            List<string> extensions = new List<string>();
            foreach (DatasetFile df in dfList)
            {
                extensions.Add(Path.GetExtension(df.FileName));
            }

            return extensions.Distinct().ToList();
        }
        /// <summary>
        /// Get file extension of file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).TrimStart('.').ToLower();
        }

        /// <summary>
        /// Return list of DatasetFileConfig object for a Dataset Id
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="dscontext"></param>
        /// <returns></returns>
        public static List<DatasetFileConfig> LoadDatasetFileConfigsByDatasetID(int datasetId, IDatasetContext dscontext)
        {
            List<DatasetFileConfig> filelist = dscontext.getAllDatasetFileConfigs().Where(w => w.ParentDataset.DatasetId == datasetId).ToList();
            return filelist;
        }
        /// <summary>
        /// Returns matching DatasetFileConfigs based on input filepath (full path)
        /// </summary>
        /// <param name="dfcList"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<DatasetFileConfig> GetMatchingDatasetFileConfigs(List<DatasetFileConfig> dfcList, string filePath)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Process input file and returns DatasetFile object.  Upload user based on FileInfo object metadata.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="dfConfig"></param>
        /// <param name="file"></param>
        /// <param name="isBundled"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static DatasetFile ProcessInputFile(Dataset dataset, DatasetFileConfig dfConfig, bool isBundled, LoaderRequest response, string file = null)
        {
            string filename = null;
            if (file != null) { filename = Path.GetFileName(file); }

            return ProcessFile(dataset, dfConfig, file, null, filename, isBundled, response);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileConfig"></param>
        /// <param name="ds"></param>
        /// <param name="response"></param>
        /// <param name="dscontext"></param>
        public static DatasetFile ProcessBundleFile(DatasetFileConfig fileConfig, Dataset ds, BundleResponse response, IDatasetContext dscontext)
        {
            throw new NotImplementedException();
        }



        private static DatasetFile ProcessFile(Dataset ds, DatasetFileConfig dfc, string fileInfo, HttpPostedFileBase filestream, string filename, bool isBundled, LoaderRequest response)
        {
            S3ServiceProvider _s3Service = new S3ServiceProvider();
            DatasetFile df_Orig = null;
            DatasetFile df_newParent = null;
            string targetFileName = null;
            string uplduser = null;
            int df_id = 0;
            RetrieverJob job = null;
            IContainer _container;
            SchemaRevision latestSchemaRevision = null;

            using (_container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _dscontext = _container.GetInstance<IDatasetContext>();
                IRequestContext _requestContext = _container.GetInstance<IRequestContext>();
                IMessagePublisher _publisher = _container.GetInstance<IMessagePublisher>();

                DateTime startTime = DateTime.Now;

                if (isBundled)
                {
                    latestSchemaRevision = dfc.GetLatestSchemaRevision();
                    Logger.Debug("ProcessFile: Detected Bundled file");
                    targetFileName = response.TargetFileName;
                    uplduser = response.RequestInitiatorId;
                    

                    //This will always overwrite an existing data file.

                    Logger.Debug("ProcessFile: Data File Config OverwriteDatafile=true");
                    // RegexSearch requires passing targetFileName to esnure we get the correct related data file.

                    df_id = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.ParentDataset.DatasetId, dfc.ConfigId, isBundled, targetFileName, latestSchemaRevision);
                    
                    //If datafiles exist for this DatasetFileConfig
                    if (df_id != 0)
                    {
                        df_Orig = _dscontext.DatasetFile_ActiveStatus.Where(x => x.DatasetFileId == df_id).Fetch(x => x.DatasetFileConfig).FirstOrDefault();
                        df_newParent = CreateParentDatasetFile(ds, dfc, uplduser, targetFileName, df_Orig, isBundled, startTime);
                    }
                    //If there are no datafiles for this DatasetFileConfig
                    else
                    {
                        df_newParent = CreateParentDatasetFile(ds, dfc, uplduser, targetFileName, null, isBundled, startTime);
                    }
                                        
                    df_newParent.IsBundled = true;
                    df_newParent.UploadUserName = response.RequestInitiatorId;
                    df_newParent.VersionId = response.TargetVersionId;
                    df_newParent.FileLocation = response.TargetKey;
                    

                    //Register new Parent DatasetFile
                    try
                    {
                        //Write dataset to database
                        _dscontext.Merge(df_newParent);
                        _dscontext.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                        StringBuilder builder = new StringBuilder();
                        builder.Append("Failed to record new Parent DatasetFile to Dataset Management.");
                        builder.Append($"File_NME: {df_newParent.FileName}");
                        builder.Append($"Dataset_ID: {df_newParent.Dataset.DatasetId}");
                        builder.Append($"UploadUser_NME: {df_newParent.UploadUserName}");
                        builder.Append($"Create_DTM: {df_newParent.CreatedDTM}");
                        builder.Append($"Modified_DTM: {df_newParent.ModifiedDTM}");
                        builder.Append($"FileLocation: {df_newParent.FileLocation}");
                        builder.Append($"Config_ID: {df_newParent.DatasetFileConfig.ConfigId}");
                        builder.Append($"ParentDatasetFile_ID: {df_newParent.ParentDatasetFileId}");
                        builder.Append($"Version_ID: {df_newParent.VersionId}");

                        Sentry.Common.Logging.Logger.Error(builder.ToString(), ex);
                        
                        //Preserve the Stack Trace
                        throw;
                    }

                    // If there were existing datasetfiles set parentdatasetFile_ID on old parent
                    if (df_id != 0)
                    {
                        try
                        {
                            //Version the Old Parent DatasetFile
                            int df_newParentId = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.ParentDataset.DatasetId, dfc.ConfigId, isBundled, null, latestSchemaRevision);
                            if (df_Orig != null)
                            {
                                df_Orig.ParentDatasetFileId = df_newParentId;
                            }
                            else
                            {
                                throw new ArgumentException("Original Datafile was not found");
                            }

                            //Write dataset to database
                            _dscontext.Merge(df_Orig);
                            _dscontext.SaveChanges();

                        }
                        catch(Exception ex)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append("Failed to set ParentDatasetFile_ID on Original Parent in Dataset Management.");
                            if (df_Orig != null)
                            {
                                builder.Append($"DatasetFile_ID: {df_Orig.DatasetFileId}");
                                builder.Append($"File_NME: {df_Orig.FileName}");
                                builder.Append($"ParentDatasetFile_ID: {df_Orig.ParentDatasetFileId}");
                            }

                            Sentry.Common.Logging.Logger.Error(builder.ToString(), ex);
                        }
                    }

                    Event f = new Event()
                    {
                        EventType = _dscontext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault(),
                        Status = _dscontext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                        UserWhoStartedEvent = response.RequestInitiatorId,
                        Dataset = response.DatasetID,
                        DataConfig = response.DatasetFileConfigId,
                        Reason = $"Successfully Uploaded file [<b>{Path.GetFileName(targetFileName)}</b>] to dataset [<b>{df_newParent.Dataset.DatasetName}</b>]",
                        Parent_Event = response.RequestGuid
                    };
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                }
                else if (!isBundled)
                {
                    if (response.RetrieverJobId > 0)
                    {
                        job = _requestContext.RetrieverJob.Where(w => w.Id == response.RetrieverJobId).FirstOrDefault();
                    }
                    else
                    {
                        throw new ArgumentException("Job ID is invalid");
                    }

                    Logger.Debug("ProcessFile: Detected Dataset file");

                    latestSchemaRevision = job.DatasetConfig.GetLatestSchemaRevision();
                    //Should this file be loaded into this config
                    if (!job.FilterIncomingFile(filename))
                    {
                        //Remove ProcessedFilePrefix from file name
                        var newFileName = filename.Replace(Configuration.Config.GetHostSetting("ProcessedFilePrefix"), "");

                        targetFileName = job.GetTargetFileName(newFileName);
                        uplduser = response.RequestInitiatorId;


                        if (job.JobOptions.OverwriteDataFile)
                        {
                            Logger.Debug("ProcessFile: Data File Config OverwriteDatafile=true");
                            // RegexSearch requires passing targetFileName to esnure we get the correct related data file.

                            if (job.JobOptions.IsRegexSearch)
                            {
                                df_id = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(job.DatasetConfig.ParentDataset.DatasetId, job.DatasetConfig.ConfigId, isBundled, targetFileName, latestSchemaRevision);
                            }
                            else
                            {
                                df_id = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(job.DatasetConfig.ParentDataset.DatasetId, job.DatasetConfig.ConfigId, isBundled, null, latestSchemaRevision);
                            }


                            //If datafiles exist for this DatasetFileConfig
                            if (df_id != 0)
                            {
                                df_Orig = _dscontext.DatasetFile_ActiveStatus.Where(x => x.DatasetFileId == df_id).Fetch(x => x.DatasetFileConfig).FirstOrDefault();
                                df_newParent = CreateParentDatasetFile(ds, job.DatasetConfig, uplduser, targetFileName, df_Orig, isBundled, startTime);
                            }
                            //If there are no datafiles for this DatasetFileConfig
                            else
                            {
                                df_newParent = CreateParentDatasetFile(ds, job.DatasetConfig, uplduser, targetFileName, null, isBundled, startTime);
                            }

                            //if the incoming data file is in the S3 drop location, then we can stay within the S3 realm and copy the object.
                            // Otherwise we need to upload the data file from the DFS drop location.
                            try
                            {
                                if (filestream == null)
                                {
                                    if (job.DataSource.Is<S3Basic>())
                                    {
                                        df_newParent.VersionId = _s3Service.CopyObject(job.DataSource.Bucket, fileInfo, job.DataSource.Bucket, df_newParent.FileLocation);

                                        ObjectKeyVersion deleteobject = _s3Service.MarkDeleted(fileInfo);
                                        Logger.Info($"Deleted S3 Drop Location Object - Delete Object(key:{deleteobject.key} versionid:{deleteobject.versionId}");
                                    }
                                    else if (job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsCustom>())
                                    {
                                        df_newParent.VersionId = _s3Service.UploadDataFile(fileInfo, df_newParent.FileLocation);
                                    }
                                    else
                                    {
                                        throw new NotImplementedException("Method not configured for DataSource Type");
                                    }
                                
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }
                            }
                            catch (AmazonS3Exception eS3)
                            {
                                Sentry.Common.Logging.Logger.Error("S3 Upload Error", eS3);
                                throw;

                            }
                            catch (Exception ex)
                            {
                                Sentry.Common.Logging.Logger.Error("Error during establishing upload process", ex);
                                throw;
                            }
                                                       
                            var diffInSeconds = (DateTime.Now - startTime).TotalSeconds;

                            if (string.IsNullOrEmpty(response.RequestInitiatorId))
                            {
                                Logger.Info($"TransferTime: {diffInSeconds} | DatasetName: {ds.DatasetName} | Initiator:null");
                            }
                            else
                            {
                                Logger.Info($"TransferTime: {diffInSeconds} | DatasetName: {ds.DatasetName} | Initiator:{response.RequestInitiatorId}");
                            }    

                            //Register new Parent DatasetFile
                            try
                            {
                                //Write dataset to database
                                _dscontext.Merge(df_newParent);
                                _dscontext.SaveChanges();
                            }
                            catch (Exception ex)
                            {

                                StringBuilder builder = new StringBuilder();
                                builder.Append("Failed to record new Parent DatasetFile to Dataset Management.");
                                builder.Append($"File_NME: {df_newParent.FileName}");
                                builder.Append($"Dataset_ID: {df_newParent.Dataset.DatasetId}");
                                builder.Append($"UploadUser_NME: {df_newParent.UploadUserName}");
                                builder.Append($"Create_DTM: {df_newParent.CreatedDTM}");
                                builder.Append($"Modified_DTM: {df_newParent.ModifiedDTM}");
                                builder.Append($"FileLocation: {df_newParent.FileLocation}");
                                builder.Append($"Config_ID: {df_newParent.DatasetFileConfig.ConfigId}");
                                builder.Append($"ParentDatasetFile_ID: {df_newParent.ParentDatasetFileId}");
                                builder.Append($"Version_ID: {df_newParent.VersionId}");

                                Sentry.Common.Logging.Logger.Error(builder.ToString(), ex);
                        
                                throw;
                            }

                            // If there were existing datasetfiles set parentdatasetFile_ID on old parent
                            if (df_id != 0)
                            {
                                try
                                {
                                    //Version the Old Parent DatasetFile
                                    int df_newParentId = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(job.DatasetConfig.ParentDataset.DatasetId, job.DatasetConfig.ConfigId, isBundled, null, latestSchemaRevision);
                                    df_Orig.ParentDatasetFileId = df_newParentId;

                                    //Write dataset to database
                                    _dscontext.Merge(df_Orig);
                                    _dscontext.SaveChanges();

                                }
                                catch (Exception ex)
                                {
                                    StringBuilder builder = new StringBuilder();
                                    builder.Append("Failed to set ParentDatasetFile_ID on Original Parent in Dataset Management.");
                                    builder.Append($"DatasetFile_ID: {df_Orig.DatasetFileId}");
                                    builder.Append($"File_NME: {df_Orig.FileName}");
                                    builder.Append($"ParentDatasetFile_ID: {df_Orig.ParentDatasetFileId}");

                                    Sentry.Common.Logging.Logger.Error(builder.ToString(), ex);
                                }
                            }

                            //Write file information to topic
                            RawFileAddModel rawFileEvent = new RawFileAddModel();
                            try
                            {
                                rawFileEvent = new RawFileAddModel()
                                {
                                    SourceBucket = RootBucket,
                                    SourceKey = df_newParent.FileLocation,
                                    SourceVersionId = df_newParent.VersionId,
                                    SchemaID = df_newParent.DatasetFileConfig.Schema.SchemaId,
                                    DatasetID = df_newParent.DatasetFileConfig.ParentDataset.DatasetId
                                };

                                _publisher.PublishDSCEvent(df_newParent.DatasetFileConfig.Schema.SchemaId.ToString(), JsonConvert.SerializeObject(rawFileEvent));
                            }
                            catch (Exception ex)
                            {
                                job.JobLoggerMessage("ERROR", $"Failed writing SCHEMA-RAWFILE-ADD event - key:{df_newParent.Schema.SchemaId.ToString()} | DSCEvent topic | message:{JsonConvert.SerializeObject(rawFileEvent)})", ex);
                            }

                            Event f = new Event()
                            {
                                EventType = _dscontext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault(),
                                Status = _dscontext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                                UserWhoStartedEvent = response.RequestInitiatorId,
                                Dataset = response.DatasetID,
                                DataConfig = response.DatasetFileConfigId,
                                Reason = $"Successfully Uploaded file [<b>{Path.GetFileName(targetFileName)}</b>] to dataset [<b>{df_newParent.Dataset.DatasetName}</b>]",
                                Parent_Event = response.RequestGuid
                            };
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                        }
                        else
                        {
                            Logger.Debug("ProcessFile: Data File Config OverwriteDatafile=false");

                            //Generating an epoch time to ensure uniqueness
                            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            var date = Convert.ToInt64((startTime.ToUniversalTime() - epoch).TotalSeconds).ToString();
                            string extension = Path.GetExtension(targetFileName);
                            string fname = Path.GetFileNameWithoutExtension(targetFileName);
                            //string outfilename = fname + "_" + date.ToString() + "_" + startTime.ToString("fff") + extension;
                            targetFileName = startTime.ToString("yyyyMMdd") + "_" + fname + "_" + GenerateHash($"{job.Id}_{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffZ")}").ToString("N").Substring(0,12) + extension;

                            df_newParent = CreateParentDatasetFile(ds, job.DatasetConfig, uplduser, targetFileName, null, isBundled, startTime);                            

                            try
                            {
                                SendFile(fileInfo, filestream, df_newParent, job);
                            }
                            catch (AmazonS3Exception eS3)
                            {
                                Sentry.Common.Logging.Logger.Error("S3 Upload Error", eS3);
                                throw;

                            }
                            catch (Exception ex)
                            {
                                Sentry.Common.Logging.Logger.Error("Error during establishing upload process", ex);
                                throw;
                            }

                            var diffInSeconds = (DateTime.Now - startTime).TotalSeconds;

                            if (string.IsNullOrEmpty(response.RequestInitiatorId))
                            {
                                Logger.Info($"TransferTime: {diffInSeconds} | DatasetName: {ds.DatasetName} | Initiator:null");
                            }
                            else
                            {
                                Logger.Info($"TransferTime: {diffInSeconds} | DatasetName: {ds.DatasetName} | Initiator:{response.RequestInitiatorId}");
                            }

                            //Register new Parent DatasetFile
                            try
                            {
                                //Write dataset to database
                                _dscontext.Merge(df_newParent);
                                _dscontext.SaveChanges();
                            }
                            catch (Exception ex)
                            {

                                StringBuilder builder = new StringBuilder();
                                builder.Append("Failed to record new Parent DatasetFile to Dataset Management.");
                                builder.Append($"File_NME: {df_newParent.FileName}");
                                builder.Append($"Dataset_ID: {df_newParent.Dataset.DatasetId}");
                                builder.Append($"UploadUser_NME: {df_newParent.UploadUserName}");
                                builder.Append($"Create_DTM: {df_newParent.CreatedDTM}");
                                builder.Append($"Modified_DTM: {df_newParent.ModifiedDTM}");
                                builder.Append($"FileLocation: {df_newParent.FileLocation}");
                                builder.Append($"Config_ID: {df_newParent.DatasetFileConfig.ConfigId}");
                                builder.Append($"ParentDatasetFile_ID: {df_newParent.ParentDatasetFileId}");
                                builder.Append($"Version_ID: {df_newParent.VersionId}");

                                Sentry.Common.Logging.Logger.Error(builder.ToString(), ex);

                                throw;
                            }

                            RawFileAddModel rawFileEvent = new RawFileAddModel();
                            //Write file information to topic
                            try
                            {
                                rawFileEvent = new RawFileAddModel()
                                {
                                    SourceBucket = RootBucket,
                                    SourceKey = df_newParent.FileLocation,
                                    SourceVersionId = df_newParent.VersionId,
                                    SchemaID = df_newParent.DatasetFileConfig.Schema.SchemaId,
                                    DatasetID = df_newParent.DatasetFileConfig.ParentDataset.DatasetId
                                };

                                _publisher.PublishDSCEvent(df_newParent.DatasetFileConfig.Schema.SchemaId.ToString(), JsonConvert.SerializeObject(rawFileEvent));
                            }
                            catch (Exception ex)
                            {
                                job.JobLoggerMessage("ERROR", $"Failed writing SCHEMA-RAWFILE-ADD event - key:{df_newParent.Schema.SchemaId.ToString()} | DSCEvent topic | message:{JsonConvert.SerializeObject(rawFileEvent)})", ex);
                            }

                            Event f = new Event()
                            {
                                EventType = _dscontext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault(),
                                Status = _dscontext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                                UserWhoStartedEvent = response.RequestInitiatorId,
                                Dataset = response.DatasetID,
                                DataConfig = response.DatasetFileConfigId,
                                Reason = $"Successfully Uploaded file [<b>{Path.GetFileName(df_newParent.FileLocation)}</b>] to dataset [<b>{df_newParent.Dataset.DatasetName}</b>]",
                                Parent_Event = response.RequestGuid
                            };
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);

                            //throw new NotImplementedException($"The Option of not Overwritting a DataFile is not implemented.  Change OverwriteDataFile_IND setting on Dataset_ID:{job.DatasetConfig.ParentDataset.DatasetId} Config_ID:{job.DatasetConfig.ConfigId} Config_Name:{job.DatasetConfig.Name}");
                        }

                        if (job.JobOptions.CreateCurrentFile)
                        {
                            Logger.Info("Creating Current File...");

                            try
                            {
                                //Create target directory if does not exist
                                Directory.CreateDirectory(job.DatasetConfig.GetCurrentFileDir().LocalPath);

                                //Delete contents of current file dir, since there should only be one file
                                // in this location at any given time.
                                foreach (string file in Directory.GetFiles(job.DatasetConfig.GetCurrentFileDir().LocalPath))
                                {
                                    File.Delete(file);
                                }

                                Logger.Debug($"Current file target : {Path.Combine(job.DatasetConfig.GetCurrentFileDir().LocalPath, targetFileName)}");

                                if (job.DataSource.Is<S3Basic>())
                                {
                                    //Stream file to work location
                                    using (Stream sourcefs = _s3Service.GetObject(df_newParent.FileLocation, df_newParent.VersionId))
                                    {
                                        //Using FileMode.Create will overwrite file if exists
                                        using (Stream targetfs = new FileStream(Path.Combine(job.DatasetConfig.GetCurrentFileDir().LocalPath, targetFileName), FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                                        {
                                            sourcefs.CopyTo(targetfs);
                                        }
                                    }
                                }
                                else if (job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsCustom>())
                                {
                                    //Copy file to current file directory
                                    //Using the overwrite option since this should only ever be the latest version
                                    File.Copy(fileInfo, Path.Combine(job.DatasetConfig.GetCurrentFileDir().LocalPath, targetFileName), true);
                                }
                                else
                                {
                                    throw new NotImplementedException("Method not configured for DataSource Type");
                                }   

                                Event f = new Event()
                                {
                                    EventType = _dscontext.EventTypes.Where(w => w.Description == "Current File Created").FirstOrDefault(),
                                    Status = _dscontext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                                    UserWhoStartedEvent = response.RequestInitiatorId,
                                    Dataset = response.DatasetID,
                                    DataConfig = response.DatasetFileConfigId,
                                    Reason = $"Successfully created new current file for [<b>{job.DatasetConfig.ParentDataset.DatasetName}</b>] dataset.",
                                    Parent_Event = response.RequestGuid
                                };
                                Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Failed to create current file", ex);

                                Event f = new Event()
                                {
                                    EventType = _dscontext.EventTypes.Where(w => w.Description == "Current File Created").FirstOrDefault(),
                                    Status = _dscontext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault(),
                                    UserWhoStartedEvent = response.RequestInitiatorId,
                                    Dataset = response.DatasetID,
                                    DataConfig = response.DatasetFileConfigId,
                                    Reason = $"Failed to created current file for [<b>{job.DatasetConfig.ParentDataset.DatasetName}</b>] dataset.",
                                    Parent_Event = response.RequestGuid
                                };
                                Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                            }
                        }
                    }
                    else
                    {
                        Logger.Info($"Filtered file from processing - Job:{job.Id} File:{filename}");                        
                    }
                }
            }

            return df_newParent;
        }

        private static void SendFile(string fileInfo, HttpPostedFileBase filestream, DatasetFile df_newParent, RetrieverJob job)
        {
            try
            {
                if (filestream == null)
                {
                    S3ServiceProvider _s3provider = new S3ServiceProvider();

                    if (job.DataSource.Is<S3Basic>())
                    {
                        df_newParent.VersionId = _s3provider.CopyObject(job.DataSource.Bucket, fileInfo, job.DataSource.Bucket, df_newParent.FileLocation);

                        ObjectKeyVersion deleteobject = _s3provider.MarkDeleted(fileInfo);
                        Logger.Info($"Deleted S3 Drop Location Object - Delete Object(key:{deleteobject.key} versionid:{deleteobject.versionId}");
                    }
                    else if (job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsCustom>())
                    {
                        df_newParent.VersionId = _s3provider.UploadDataFile(fileInfo, df_newParent.FileLocation);
                    }
                    else
                    {
                        throw new NotImplementedException("Method not configured for DataSource Type");
                    }

                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (AmazonS3Exception eS3)
            {
                Sentry.Common.Logging.Logger.Error("S3 Upload Error", eS3);
                throw;

            }
            catch (Exception ex)
            {
                Sentry.Common.Logging.Logger.Error("Error during establishing upload process", ex);
                throw;
            }
        }

        private static DatasetFile CreateParentDatasetFile(Dataset ds, DatasetFileConfig dfc, string uploaduser, string targetFileName, DatasetFile CurrentDatasetFile, bool isbundle, DateTime processingTime)
        {
            DatasetFile out_df = null;
            string fileLocation = null;

            string fileOwner = uploaduser;


            if (isbundle)
            {
                StringBuilder location = new StringBuilder();
                location.Append(Configuration.Config.GetHostSetting("S3BundlePrefix"));
                location.Append(GenerateCustomStorageLocation(new string[] { ds.DatasetCategories.First().Id.ToString(), ds.DatasetId.ToString() }));
                location.Append(targetFileName);
                fileLocation = location.ToString();
            }
            else
            {
                if (CurrentDatasetFile == null)
                {
                    fileLocation = Utilities.GenerateDatafileKey(ds, processingTime, targetFileName, dfc);
                }
                else
                {
                    fileLocation = CurrentDatasetFile.FileLocation;
                }
            }

            out_df = new DatasetFile()
            {
               DatasetFileId = 0,
               FileName = targetFileName,
               Dataset = ds,
               UploadUserName = fileOwner,
               DatasetFileConfig = dfc,
               FileLocation = fileLocation,
               CreatedDTM = processingTime,
               ModifiedDTM = processingTime,
               ParentDatasetFileId = null,
               VersionId = null,
               IsBundled = isbundle,
               Size = 0,
               SchemaRevision = dfc.GetLatestSchemaRevision(),
               Schema = dfc.Schema
            };

            return out_df;
        }
        /// <summary>
        /// Return File Owner name
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static string GetFileOwner(FileInfo fileInfo)
        {
            var fs = File.GetAccessControl(fileInfo.FullName);
            var sid = fs.GetOwner(typeof(SecurityIdentifier));
            var ntAccount = sid.Translate(typeof(NTAccount));

            //remove domain
            var outowner = ntAccount.ToString().Replace(@"SHOESD01\", "");

            return outowner;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="df"></param>
        /// <param name="fi"></param>
        public static void RemoveProcessedFile(DatasetFile df, FileInfo fi)
        {
            try
            {
                File.Delete(fi.FullName);
            }
            catch (Exception e)
            {
                //Allow application to continue without error.  Log message for BI Portal SOS group.
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Failed to delete input file:");
                builder.AppendLine($"DatasetFile_ID: {df.DatasetFileId}");
                builder.AppendLine($"File_NME: {df.FileName}");
                builder.AppendLine($"File Location: {fi.FullName}");

                Sentry.Common.Logging.Logger.Error(builder.ToString(), e);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static Boolean IsExtentionPreviewCompatible(string extension)
        {
            switch (extension)
            {
                case "csv":
                case "txt":
                case "json":
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static Boolean IsExtentionPushToSAScompatible(string extension)
        {
            switch (extension)
            {
                case "csv":
                case ".csv":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Generates hash based on input, returns GUID 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Guid GenerateHash(string input)
        {
            Guid result;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                result = new Guid(hash);                
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public static async Task CreateEventAsync(Event e)
        {
            IContainer _container;
            using (_container = Bootstrapper.Container.GetNestedContainer())
            {
                var _datasetContext = _container.GetInstance<IDatasetContext>();

                try
                {
                    _datasetContext.Merge<Event>(e);
                    _datasetContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to save event", ex);
                }

            }
        }
    }

    public static class DirectoryUtilities
    {
        /// <summary>
        /// Utilizes the Windows API to return a list of effective permissions for given user on a directory path
        /// </summary>
        /// <param name="user"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> EffectivePermissions(string user, string path)
        {
            //String UserName = "NT Authority\\Authenticated Users";

            List<string> results = new List<string>();

            IntPtr pSidOwner, pSidGroup, pDacl, pSacl, pSecurityDescriptor;
            ACCESS_MASK mask = new ACCESS_MASK();
            uint ret = GetNamedSecurityInfo(path,
                SE_OBJECT_TYPE.SE_FILE_OBJECT,
                SECURITY_INFORMATION.DACL_SECURITY_INFORMATION | SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION | SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION,
                out pSidOwner, out pSidGroup, out pDacl, out pSacl, out pSecurityDescriptor);

            IntPtr hManager = IntPtr.Zero;


            bool f = AuthzInitializeResourceManager(1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, null, out hManager);

            NTAccount ac = new NTAccount(user);
            SecurityIdentifier sid = (SecurityIdentifier)ac.Translate(typeof(SecurityIdentifier));
            byte[] bytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(bytes, 0);
            String _psUserSid = "";
            foreach (byte si in bytes)
            {
                _psUserSid += si;
            }

            LUID unusedSid = new LUID();
            IntPtr UserSid = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, UserSid, bytes.Length);
            IntPtr pClientContext = IntPtr.Zero;

            if (f)
            {
                f = AuthzInitializeContextFromSid(0, UserSid, hManager, IntPtr.Zero, unusedSid, IntPtr.Zero, out pClientContext);

                AUTHZ_ACCESS_REQUEST request = new AUTHZ_ACCESS_REQUEST();
                request.DesiredAccess = 0x02000000;
                request.PrincipalSelfSid = null;
                request.ObjectTypeList = null;
                request.ObjectTypeListLength = 0;
                request.OptionalArguments = IntPtr.Zero;

                AUTHZ_ACCESS_REPLY reply = new AUTHZ_ACCESS_REPLY();
                reply.GrantedAccessMask = IntPtr.Zero;
                reply.ResultListLength = 0;
                reply.SaclEvaluationResults = IntPtr.Zero;
                IntPtr AccessReply = IntPtr.Zero;
                reply.Error = Marshal.AllocHGlobal(1020);
                reply.GrantedAccessMask = Marshal.AllocHGlobal(sizeof(uint));
                reply.ResultListLength = 1;
                int i = 0;
                Dictionary<String, String> rightsmap = new Dictionary<String, String>();
                List<string> effectivePermissionList = new List<string>();
                                
                rightsmap.Add("FILE_TRAVERSE", "Traverse_Folder_and_Execute_File");
                rightsmap.Add("FILE_LIST_DIRECTORY", "List_Folder_and_Read_Data");
                rightsmap.Add("FILE_READ_DATA", "List_Folder_and_Read_Data");
                rightsmap.Add("FILE_READ_ATTRIBUTES", "Read_Attributes");
                rightsmap.Add("FILE_READ_EA", "Read_Extended_Attributes");
                rightsmap.Add("FILE_ADD_FILE", "Create_Files_and_Write_Files");
                rightsmap.Add("FILE_WRITE_DATA", "Create_Files_and_Write_Files");
                rightsmap.Add("FILE_ADD_SUBDIRECTORY", "Create_Folders_and_Append_Data");
                rightsmap.Add("FILE_APPEND_DATA", "Create_Folders_and_Append_Data");
                rightsmap.Add("FILE_WRITE_ATTRIBUTES", "Write_Attributes");
                rightsmap.Add("FILE_WRITE_EA", "Write_Extended_Attributes");
                rightsmap.Add("FILE_DELETE_CHILD", "Delete_Subfolders_and_Files");
                rightsmap.Add("DELETE", "Delete");
                rightsmap.Add("READ_CONTROL", "Read_Permission");
                rightsmap.Add("WRITE_DAC", "Change_Permission");
                rightsmap.Add("WRITE_OWNER", "Take_Ownership");


                f = AuthzAccessCheck(0, pClientContext, ref request, IntPtr.Zero, pSecurityDescriptor, null, 0, ref reply, out AccessReply);
                if (f)
                {
                    int granted_access = Marshal.ReadInt32(reply.GrantedAccessMask);

                    mask = (ACCESS_MASK)granted_access;

                    foreach (ACCESS_MASK item in Enum.GetValues(typeof(ACCESS_MASK)))
                    {
                        if ((mask & item) == item)
                        {
                            effectivePermissionList.Add(rightsmap[item.ToString()]);
                            i++;
                        }

                    }
                }
                Marshal.FreeHGlobal(reply.GrantedAccessMask);


                if (i == 16)
                {
                    effectivePermissionList.Insert(0, "Full_Control");
                }

                foreach (AccessRights r in Enum.GetValues(typeof(AccessRights)))
                {
                    if (effectivePermissionList.Contains(r.ToString()))
                    {
                        results.Add(r.ToString());
                    }
                }
            }

            return results;

        }

        /// <summary>
        /// Determines if user has all AccessRights on specified path.  Will return false if
        /// only paritial AccessRights are found.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="path"></param>
        /// <param name="rights"></param>
        /// <returns></returns>
        public static Boolean HasPermission(string user, string path, List<AccessRights> rights)
        {
            List<string> folderRights = EffectivePermissions(user, path);
            //List<string> expectedRights = ;

            //rights.Except(folderRights);

            //foreach (string right in )
            //{
            //    if (Enum.IsDefined(typeof(AccessRights), right))
            //    {
            //        foundRights.Add(right);
            //    }
            //}

            return (rights.Select(s => s.ToString()).Except(folderRights).Any()) ? false : true;

        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern uint GetEffectiveRightsFromAcl(IntPtr pDacl, ref TRUSTEE pTrustee, ref ACCESS_MASK pAccessRights);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        private struct TRUSTEE
        {
            IntPtr pMultipleTrustee; // must be null
            public int MultipleTrusteeOperation;
            public TRUSTEE_FORM TrusteeForm;
            public TRUSTEE_TYPE TrusteeType;
            [MarshalAs(UnmanagedType.LPStr)]
            public string ptstrName;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AUTHZ_ACCESS_REQUEST
        {
            public int DesiredAccess;
            public byte[] PrincipalSelfSid;
            public OBJECT_TYPE_LIST[] ObjectTypeList;
            public int ObjectTypeListLength;
            public IntPtr OptionalArguments;
        };
        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_TYPE_LIST
        {
            OBJECT_TYPE_LEVEL Level;
            int Sbz;
            IntPtr ObjectType;
        };

        [StructLayout(LayoutKind.Sequential)]
        private struct AUTHZ_ACCESS_REPLY
        {
            public int ResultListLength;
            public IntPtr GrantedAccessMask;
            public IntPtr SaclEvaluationResults;
            public IntPtr Error;
        };

        private enum OBJECT_TYPE_LEVEL : int
        {
            ACCESS_OBJECT_GUID = 0,
            ACCESS_PROPERTY_SET_GUID = 1,
            ACCESS_PROPERTY_GUID = 2,
            ACCESS_MAX_LEVEL = 4
        };
        private enum TRUSTEE_FORM
        {
            TRUSTEE_IS_SID,
            TRUSTEE_IS_NAME,
            TRUSTEE_BAD_FORM,
            TRUSTEE_IS_OBJECTS_AND_SID,
            TRUSTEE_IS_OBJECTS_AND_NAME
        }

        private enum AUTHZ_RM_FLAG : uint
        {
            AUTHZ_RM_FLAG_NO_AUDIT = 1,
            AUTHZ_RM_FLAG_INITIALIZE_UNDER_IMPERSONATION = 2,
            AUTHZ_RM_FLAG_NO_CENTRAL_ACCESS_POLICIES = 4,
        }

        private enum TRUSTEE_TYPE
        {
            TRUSTEE_IS_UNKNOWN,
            TRUSTEE_IS_USER,
            TRUSTEE_IS_GROUP,
            TRUSTEE_IS_DOMAIN,
            TRUSTEE_IS_ALIAS,
            TRUSTEE_IS_WELL_KNOWN_GROUP,
            TRUSTEE_IS_DELETED,
            TRUSTEE_IS_INVALID,
            TRUSTEE_IS_COMPUTER
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        static extern uint GetNamedSecurityInfo(
            string pObjectName,
            SE_OBJECT_TYPE ObjectType,
            SECURITY_INFORMATION SecurityInfo,
            out IntPtr pSidOwner,
            out IntPtr pSidGroup,
            out IntPtr pDacl,
            out IntPtr pSacl,
            out IntPtr pSecurityDescriptor);
        [DllImport("authz.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, EntryPoint = "AuthzInitializeContextFromSid", CharSet = CharSet.Unicode)]
        static extern private bool AuthzInitializeContextFromSid(
                                               int Flags,
                                               IntPtr UserSid,
                                               IntPtr AuthzResourceManager,
                                               IntPtr pExpirationTime,
                                               LUID Identitifier,
                                               IntPtr DynamicGroupArgs,
                                               out IntPtr pAuthzClientContext
                                               );



        [DllImport("authz.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, EntryPoint = "AuthzInitializeResourceManager", CharSet = CharSet.Unicode)]
        static extern private bool AuthzInitializeResourceManager(
                                        int flags,
                                        IntPtr pfnAccessCheck,
                                        IntPtr pfnComputeDynamicGroups,
                                        IntPtr pfnFreeDynamicGroups,
                                        string name,
                                        out IntPtr rm
                                        );
        [DllImport("authz.dll", EntryPoint = "AuthzAccessCheck", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool AuthzAccessCheck(int flags,
                                                    IntPtr hAuthzClientContext,
                                                     ref AUTHZ_ACCESS_REQUEST pRequest,
                                                     IntPtr AuditEvent,
                                                     IntPtr pSecurityDescriptor,
                                                    byte[] OptionalSecurityDescriptorArray,
                                                    int OptionalSecurityDescriptorCount,
                                                    ref AUTHZ_ACCESS_REPLY pReply,
                                                    out IntPtr phAccessCheckResults);

        private enum ACCESS_MASK : uint
        {
            FILE_TRAVERSE = 0x20,
            FILE_LIST_DIRECTORY = 0x1,
            FILE_READ_DATA = 0x1,
            FILE_READ_ATTRIBUTES = 0x80,
            FILE_READ_EA = 0x8,
            FILE_ADD_FILE = 0x2,
            FILE_WRITE_DATA = 0x2,
            FILE_ADD_SUBDIRECTORY = 0x4,
            FILE_APPEND_DATA = 0x4,
            FILE_WRITE_ATTRIBUTES = 0x100,
            FILE_WRITE_EA = 0x10,
            FILE_DELETE_CHILD = 0x40,
            DELETE = 0x10000,
            READ_CONTROL = 0x20000,
            WRITE_DAC = 0x40000,
            WRITE_OWNER = 0x80000,


            ////////FILE_EXECUTE =0x20,   
        }

        [Flags]
        private enum SECURITY_INFORMATION : uint
        {
            OWNER_SECURITY_INFORMATION = 0x00000001,
            GROUP_SECURITY_INFORMATION = 0x00000002,
            DACL_SECURITY_INFORMATION = 0x00000004,
            SACL_SECURITY_INFORMATION = 0x00000008,
            UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
            UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
            PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
            PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000
        }

        private enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY
        }


        public enum AccessRights
        {
            Full_Control = 0,
            Traverse_Folder_and_Execute_File = 1,
            Read_Attributes = 2,
            Read_Extended_Attributes = 3,
            Create_Files_and_Write_Files = 4,
            Create_Folders_and_Append_Data = 5,
            Write_Attributes = 6,
            Write_Extended_Attributes = 7,
            Delete_Subfolders_and_Files = 8,
            Delete = 9,
            Read_Permission = 10,
            Change_Permission = 11,
            Take_Ownership = 12,
            List_Folder_and_Read_Data = 13
        }
    }
}

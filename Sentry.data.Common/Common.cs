using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Principal;
using Amazon.S3;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using Sentry.data.Infrastructure;
using System.Security.Cryptography;
using Sentry.Common.Logging;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Sentry.data.Common
{
    /// <summary>
    /// Provides common code between projects
    /// </summary>
    /// 
    public static class Utilities
    {        


        private static S3ServiceProvider _s3Service = new S3ServiceProvider();
        private static StructureMap.IContainer _container = null;

        /// <summary>
        /// Generates full drop location path for a dataset
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string GenerateDatasetDropLocation(Dataset ds)
        {
            string filep = Path.Combine(Configuration.Config.GetHostSetting("DatasetLoaderBaseLocation"), ds.DatasetCategory.Name.ToLower());
            filep = Path.Combine(filep, ds.DatasetName.Replace(' ', '_').ToLower());
            //filep = Path.Combine(filep, GenerateDatasetFrequencyLocationName(ds.CreationFreqDesc).ToLower());
            return filep.ToString();
        }
        /// <summary>
        /// Generates full drop location path for a dataset.  <i>Specify all parameters</i>
        /// </summary>
        /// <param name="creationFrequency"></param>
        /// <param name="categoryName"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static string GenerateDatasetDropLocation(string categoryName, string datasetName)
        {
            string filep = Path.Combine(Configuration.Config.GetHostSetting("DatasetLoaderBaseLocation"), categoryName.ToLower());
            filep = Path.Combine(filep, datasetName.Replace(' ', '_').ToLower()) + @"\";
            //filep = Path.Combine(filep, creationFrequency.Replace(' ', '_').ToLower());
            return filep.ToString();
        }

        /// <summary>
        /// Generate storage location path for dataset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ds"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static string GenerateDatasetStorageLocation(Dataset ds)
        {
            return GenerateLocationKey(ds.DatasetCategory.Name, ds.DatasetName);
        }
        /// <summary>
        /// Generate storage location path.  <i>Specify all parameters</i>
        /// </summary>
        /// <param name="creationFrequency"></param>
        /// <param name="categoryName"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static string GenerateDatasetStorageLocation(string categoryName, string datasetName)
        {
            return GenerateLocationKey(categoryName, datasetName);
        }
        /// <summary>
        /// Generate storage key for datafile
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="now"></param>
        /// <param name="filename"></param>
        /// <param name="dataFileConfigId"></param>
        /// <returns></returns>
        public static string GenerateDatafileKey(Dataset ds, DateTime now, string filename, int dataFileConfigId)
        {
            StringBuilder location = new StringBuilder();
            location.Append(GenerateDatasetStorageLocation(ds));
            location.Append(dataFileConfigId.ToString() + '/');
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
        /// <param name="creationFreqDesc"></param>
        /// <param name="category"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static string GenerateLocationKey(string category, string datasetName)
        {
            StringBuilder location = new StringBuilder();
            location.Append(Configuration.Config.GetHostSetting("S3DataPrefix"));
            location.Append(category.ToLower());
            location.Append('/');
            location.Append(FormatDatasetName(datasetName));
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
        /// Return list of DatasetFileConfig objects by DropPath
        /// </summary>
        /// <param name="SystemDir"></param>
        /// <param name="dscontext"></param>
        /// <returns></returns>
        public static List<DatasetFileConfig> LoadDatasetFileConfigsByDir(string SystemDir, IDatasetContext dscontext)
        {
            List<DatasetFileConfig> filelist = dscontext.getAllDatasetFileConfigs().Where(w => w.DropPath.ToLower() == SystemDir.ToLower()).ToList();
            return filelist;
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
            List<DatasetFileConfig> outList = new List<DatasetFileConfig>();

            foreach (DatasetFileConfig fc in dfcList)
            {
                //if (!(String.IsNullOrEmpty(fc.fileSearch.fileName)))
                if (!(fc.IsRegexSearch))
                {
                    //if (Regex.IsMatch(_path, fc.fileSearch.fileName)) { configMatch++; }
                    if (Path.GetFileName(filePath) == fc.SearchCriteria) { outList.Add(fc); }
                }
                else
                {
                    if (Regex.IsMatch(Path.GetFileName(filePath), fc.SearchCriteria)) { outList.Add(fc); }
                }
            }

            return outList;
        }
        /// <summary>
        /// Process input file and returns DatasetFile object.  Upload user based on FileInfo object metadata.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="dfConfig"></param>
        /// <param name="dscontext"></param>
        /// <param name="file"></param>
        /// <param name="uploadUserName"></param>
        /// <param name="isBundled"></param>
        /// <param name="response"></param>
        /// <param name="retrieverJobId"></param>
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
            //DatasetFile df_Orig = null;
            //DatasetFile df_newParent = null;
            //string targetFileName = null;
            //int df_id = 0;


            //targetFileName = response.TargetFileName;

            //df_id = dscontext.GetLatestBundleFileIdForDatasetByDatasetFileConfig(fileConfig.ParentDataset.DatasetId, fileConfig.ConfigId);

            //df_newParent = CreateParentDatasetFile(ds, fileConfig, response.RequestInitiatorId, targetFileName, null, true);

            //////If datafiles exist for this DatasetFileConfig
            ////if (df_id != 0)
            ////{
            ////    df_Orig = dscontext.GetDatasetFile(df_id);
            ////    df_newParent = Utilities.CreateParentDatasetFile(ds, fileConfig, response.RequestInitiatorId, targetFileName, df_Orig, true);                
            ////}
            //////If there are no datafiles for this DatasetFileConfig
            ////else
            ////{
            ////    df_newParent = CreateParentDatasetFile(ds, fileConfig, response.RequestInitiatorId, targetFileName, null, true);
            ////}

            //df_newParent.IsBundled = true;
            //df_newParent.UploadUserName = response.RequestInitiatorId;
            //df_newParent.VersionId = response.TargetVersionId;
            //df_newParent.S3Key = response.TargetKey;

            ////Register new Parent DatasetFile
            //try
            //{
            //    //Write dataset to database
            //    dscontext.Merge(df_newParent);
            //    dscontext.SaveChanges();
            //}
            //catch (Exception e)
            //{

            //    StringBuilder builder = new StringBuilder();
            //    builder.Append("Failed to record new Bundle DatasetFile to Dataset Management.");
            //    builder.Append($"File_NME: {df_newParent.FileName}");
            //    builder.Append($"Dataset_ID: {df_newParent.Dataset.DatasetId}");
            //    builder.Append($"UploadUser_NME: {df_newParent.UploadUserName}");
            //    builder.Append($"Create_DTM: {df_newParent.CreateDTM}");
            //    builder.Append($"Modified_DTM: {df_newParent.ModifiedDTM}");
            //    builder.Append($"FileLocation: {df_newParent.FileLocation}");
            //    builder.Append($"DataFileConfig_ID: {df_newParent.DatasetFileConfig.ConfigId}");
            //    builder.Append($"ParentDatasetFile_ID: {df_newParent.ParentDatasetFileId}");
            //    builder.Append($"Version_ID: {df_newParent.VersionId}");

            //    Sentry.Common.Logging.Logger.Error(builder.ToString(), e);

            //    //SendNotification(datasetMetadata, (int)ExitCodes.DatabaseError, 0, "Error saving dataset to database", df_newParent.FileLocation);
            //    throw new Exception("Error saving bundle datafile to database", e);
            //    //return (int)ExitCodes.DatabaseError;
            //}

            //// If there were existing datasetfiles set parentdatasetFile_ID on old parent
            //if (df_id != 0)
            //{
            //    try
            //    {
            //        //Version the Old Parent DatasetFile
            //        //int df_newParentId = dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(fileConfig.ParentDataset.DatasetId, fileConfig.ConfigId);
            //        //df_Orig.ParentDatasetFileId = df_newParentId;

            //        //Write dataset to database
            //        dscontext.Merge(df_Orig);
            //        dscontext.SaveChanges();

            //    }
            //    catch (Exception e)
            //    {
            //        StringBuilder builder = new StringBuilder();
            //        builder.Append("Failed to set ParentDatasetFile_ID on Original Parent in Dataset Management.");
            //        builder.Append($"DatasetFile_ID: {df_Orig.DatasetFileId}");
            //        builder.Append($"File_NME: {df_Orig.FileName}");
            //        builder.Append($"ParentDatasetFile_ID: {df_Orig.ParentDatasetFileId}");
            //    }
            //}

            //return df_newParent;
        }



        private static DatasetFile ProcessFile(Dataset ds, DatasetFileConfig dfc, string fileInfo, HttpPostedFileBase filestream, string filename, bool isBundled, LoaderRequest response)
        {
            DatasetFile df_Orig = null;
            DatasetFile df_newParent = null;
            string targetFileName = null;
            string uplduser = null;
            int df_id = 0;
            RetrieverJob job = null;

            using (_container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _dscontext = _container.GetInstance<IDatasetContext>();
                IRequestContext _requestContext = _container.GetInstance<IRequestContext>();
            
                if (isBundled)
                {
                    Logger.Debug("ProcessFile: Detected Bundled file");
                    targetFileName = response.TargetFileName;
                    uplduser = response.RequestInitiatorId;

                    if (dfc.OverwriteDatafile)
                    {
                        Logger.Debug("ProcessFile: Data File Config OverwriteDatafile=true");

                        // RegexSearch requires passing targetFileName to esnure we get the correct related data file.
                        if (dfc.IsRegexSearch)
                        {
                            df_id = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.ParentDataset.DatasetId, dfc.ConfigId, targetFileName, isBundled);
                        }
                        else
                        {
                            df_id = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.ParentDataset.DatasetId, dfc.ConfigId, isBundled);
                        }   

                        //If datafiles exist for this DatasetFileConfig
                        if (df_id != 0)
                        {
                            df_Orig = _dscontext.GetDatasetFile(df_id);
                            df_newParent = CreateParentDatasetFile(ds, dfc, uplduser, targetFileName, df_Orig, isBundled);
                        }
                        //If there are no datafiles for this DatasetFileConfig
                        else
                        {
                            df_newParent = CreateParentDatasetFile(ds, dfc, uplduser, targetFileName, null, isBundled);
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
                            builder.Append($"Create_DTM: {df_newParent.CreateDTM}");
                            builder.Append($"Modified_DTM: {df_newParent.ModifiedDTM}");
                            builder.Append($"FileLocation: {df_newParent.FileLocation}");
                            builder.Append($"Config_ID: {df_newParent.DatasetFileConfig.ConfigId}");
                            builder.Append($"ParentDatasetFile_ID: {df_newParent.ParentDatasetFileId}");
                            builder.Append($"Version_ID: {df_newParent.VersionId}");

                            Sentry.Common.Logging.Logger.Error(builder.ToString(), ex);
                        
                            throw new Exception("Error saving dataset to database", ex);
                        }

                        // If there were existing datasetfiles set parentdatasetFile_ID on old parent
                        if (df_id != 0)
                        {
                            try
                            {
                                //Version the Old Parent DatasetFile
                                int df_newParentId = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.ParentDataset.DatasetId, dfc.ConfigId, isBundled);
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
                            }
                        }

                        Event f = new Event()
                        {
                            EventType = _dscontext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault(),
                            Status = _dscontext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                            TimeCreated = DateTime.Now,
                            TimeNotified = DateTime.Now,
                            IsProcessed = false,
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
                        throw new NotImplementedException($"The Option of not Overwritting a DataFile is not implemented.  Change OverwriteDataFile_IND setting on Dataset_ID:{dfc.ParentDataset.DatasetId} Config_ID:{dfc.ConfigId} Config_Name:{dfc.Name}");
                    }
                }
                else if (!isBundled)
                {
                    Logger.Debug("ProcessFile: Detected Dataset file");

                    if (response.RetrieverJobId > 0)
                    {
                        job = _requestContext.RetrieverJob.Where(w => w.Id == response.RetrieverJobId).FirstOrDefault();
                    }

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

                            if (dfc.IsRegexSearch)
                            {
                                df_id = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(job.DatasetConfig.ParentDataset.DatasetId, job.DatasetConfig.ConfigId, targetFileName, isBundled);
                            }
                            else
                            {
                                df_id = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(job.DatasetConfig.ParentDataset.DatasetId, job.DatasetConfig.ConfigId, isBundled);
                            }


                            //If datafiles exist for this DatasetFileConfig
                            if (df_id != 0)
                            {
                                df_Orig = _dscontext.GetDatasetFile(df_id);
                                df_newParent = CreateParentDatasetFile(ds, job.DatasetConfig, uplduser, targetFileName, df_Orig, isBundled);
                            }
                            //If there are no datafiles for this DatasetFileConfig
                            else
                            {
                                df_newParent = CreateParentDatasetFile(ds, job.DatasetConfig, uplduser, targetFileName, null, isBundled);
                            }

                            DateTime startUploadTime = DateTime.Now;


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
                                throw new Exception("S3 Upload Error", eS3);

                            }
                            catch (Exception ex)
                            {
                                Sentry.Common.Logging.Logger.Error("Error during establishing upload process", ex);
                                throw new Exception("Error during establishing upload process", ex);
                            }
                    
                            var diffInSeconds = (DateTime.Now - startUploadTime).TotalSeconds;
                            Sentry.Common.Logging.Logger.Info($"TransferTime: {diffInSeconds} | DatasetName: {ds.DatasetName}");

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
                                builder.Append($"Create_DTM: {df_newParent.CreateDTM}");
                                builder.Append($"Modified_DTM: {df_newParent.ModifiedDTM}");
                                builder.Append($"FileLocation: {df_newParent.FileLocation}");
                                builder.Append($"Config_ID: {df_newParent.DatasetFileConfig.ConfigId}");
                                builder.Append($"ParentDatasetFile_ID: {df_newParent.ParentDatasetFileId}");
                                builder.Append($"Version_ID: {df_newParent.VersionId}");

                                Sentry.Common.Logging.Logger.Error(builder.ToString(), ex);
                        
                                throw new Exception("Error saving dataset to database", ex);
                            }

                            // If there were existing datasetfiles set parentdatasetFile_ID on old parent
                            if (df_id != 0)
                            {
                                try
                                {
                                    //Version the Old Parent DatasetFile
                                    int df_newParentId = _dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(job.DatasetConfig.ParentDataset.DatasetId, job.DatasetConfig.ConfigId, isBundled);
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
                                }
                            }

                            Event f = new Event()
                            {
                                EventType = _dscontext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault(),
                                Status = _dscontext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault(),
                                TimeCreated = DateTime.Now,
                                TimeNotified = DateTime.Now,
                                IsProcessed = false,
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
                            throw new NotImplementedException($"The Option of not Overwritting a DataFile is not implemented.  Change OverwriteDataFile_IND setting on Dataset_ID:{job.DatasetConfig.ParentDataset.DatasetId} Config_ID:{job.DatasetConfig.ConfigId} Config_Name:{job.DatasetConfig.Name}");
                        }

                        if (job.JobOptions.CreateCurrentFile && job.DataSource.Is<S3Basic>())
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
                                    using (Stream sourcefs = _s3Service.GetObject(df_newParent.S3Key, df_newParent.VersionId))
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
                                    TimeCreated = DateTime.Now,
                                    TimeNotified = DateTime.Now,
                                    IsProcessed = false,
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
                                    TimeCreated = DateTime.Now,
                                    TimeNotified = DateTime.Now,
                                    IsProcessed = false,
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

            /*
            // Set values appropriately based on isbundled
            if (isBundled)
            {
                Logger.Debug("ProcessFile: Detected Bundled file");
                targetFileName = response.TargetFileName;
                uplduser = response.RequestInitiatorId;
            }
            else
            {
                Logger.Debug("ProcessFile: Detected Dataset file");

                //Remove ProcessedFilePrefix from file name
                var newFileName = filename.Replace(Configuration.Config.GetHostSetting("ProcessedFilePrefix"), "");

                targetFileName = GetTargetFileName(dfc, newFileName);
                uplduser = response.RequestInitiatorId;
            }
            
            if (dfc.OverwriteDatafile)
            {
                Logger.Debug("ProcessFile: Data File Config OverwriteDatafile=true");
                // RegexSearch requires passing targetFileName to esnure we get the correct related data file.

                if (dfc.IsRegexSearch)
                {
                    df_id = dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.ParentDataset.DatasetId, dfc.ConfigId, targetFileName, isBundled);
                }
                else
                {
                    df_id = dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.ParentDataset.DatasetId, dfc.ConfigId, isBundled);
                }


                //If datafiles exist for this DatasetFileConfig
                if (df_id != 0)
                {
                    df_Orig = dscontext.GetDatasetFile(df_id);
                    df_newParent = CreateParentDatasetFile(ds, dfc, uplduser, targetFileName, df_Orig, isBundled);
                    //df_updated = updateOverwrittenDatasetFile(ds, df_Orig, fileInfo);
                }
                //If there are no datafiles for this DatasetFileConfig
                else
                {
                    df_newParent = CreateParentDatasetFile(ds, dfc, uplduser, targetFileName, null, isBundled);
                }


                if(!isBundled)
                {
                    DateTime startUploadTime = DateTime.Now;
                    // Upload new key 
                    try
                    {
                        if (filestream == null)
                        {
                            df_newParent.VersionId = _s3Service.UploadDataFile(fileInfo.FullName, df_newParent.FileLocation);
                        }
                        else
                        {
                            throw new NotImplementedException();
                            //_s3Service.TransferUtlityUploadStream(df_newParent.FileLocation, df_newParent.FileName, filestream.InputStream);
                        }
                    }
                    catch (AmazonS3Exception eS3)
                    {
                        Sentry.Common.Logging.Logger.Error("S3 Upload Error", eS3);
                        //SendNotification(datasetMetadata, (int)ExitCodes.S3UploadError, 0, "S3 Upload Error", df_newParent.FileLocation);
                        throw new Exception("S3 Upload Error", eS3);
                        //return (int)ExitCodes.S3UploadError;

                    }
                    catch (Exception ex)
                    {
                        Sentry.Common.Logging.Logger.Error("Error during establishing upload process", ex);
                        //SendNotification(datasetMetadata, (int)ExitCodes.Failure, 0, "Error during establishing upload process", df_newParent.FileLocation);
                        throw new Exception("Error during establishing upload process", ex);
                        //return (int)ExitCodes.Failure;
                    }

                    //PushFileToStorage(ds, datasetMetadata, upload, dscontext, fileInfo);
                    var diffInSeconds = (DateTime.Now - startUploadTime).TotalSeconds;
                    Sentry.Common.Logging.Logger.Info($"TransferTime: {diffInSeconds} | DatasetName: {ds.DatasetName}");
                }
                else
                {
                    df_newParent.IsBundled = true;
                    df_newParent.UploadUserName = response.RequestInitiatorId;
                    df_newParent.VersionId = response.TargetVersionId;
                    df_newParent.FileLocation = response.TargetKey;
                }
                

                ////retrieve target file size
                //try
                //{
                //     resp = _s3Service.GetObjectMetadata(df_newParent.S3Key, df_newParent.VersionId);
                //}
                //catch (AmazonS3Exception eS3)
                //{
                //    Sentry.Common.Logging.Logger.Error("Error Retrieving S3 Object Metadata", eS3);
                //    throw new Exception("S3 Object Metadata Retrieval Error", eS3);
                //}
                //catch (Exception e)
                //{
                //    Sentry.Common.Logging.Logger.Error("Error Retrieving S3 Object Metadata", e);
                //    throw new Exception("Error Retrieving S3 Object Metadata", e);
                //}


                //df_newParent.Size = Convert.ToInt64(resp["ContentLength"]);


                //Register new Parent DatasetFile
                try
                {
                    //Write dataset to database
                    dscontext.Merge(df_newParent);
                    dscontext.SaveChanges();
                }
                catch (Exception ex)
                {

                    StringBuilder builder = new StringBuilder();
                    builder.Append("Failed to record new Parent DatasetFile to Dataset Management.");
                    builder.Append($"File_NME: {df_newParent.FileName}");
                    builder.Append($"Dataset_ID: {df_newParent.Dataset.DatasetId}");
                    builder.Append($"UploadUser_NME: {df_newParent.UploadUserName}");
                    builder.Append($"Create_DTM: {df_newParent.CreateDTM}");
                    builder.Append($"Modified_DTM: {df_newParent.ModifiedDTM}");
                    builder.Append($"FileLocation: {df_newParent.FileLocation}");
                    builder.Append($"Config_ID: {df_newParent.DatasetFileConfig.ConfigId}");
                    builder.Append($"ParentDatasetFile_ID: {df_newParent.ParentDatasetFileId}");
                    builder.Append($"Version_ID: {df_newParent.VersionId}");

                    Sentry.Common.Logging.Logger.Error(builder.ToString(), ex);

                    //SendNotification(datasetMetadata, (int)ExitCodes.DatabaseError, 0, "Error saving dataset to database", df_newParent.FileLocation);
                    throw new Exception("Error saving dataset to database", ex);
                    //return (int)ExitCodes.DatabaseError;
                }

                // If there were existing datasetfiles set parentdatasetFile_ID on old parent
                if (df_id != 0)
                {
                    try
                    {
                        //Version the Old Parent DatasetFile
                        int df_newParentId = dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.ParentDataset.DatasetId, dfc.ConfigId, isBundled);
                        df_Orig.ParentDatasetFileId = df_newParentId;

                        //Write dataset to database
                        dscontext.Merge(df_Orig);
                        dscontext.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append("Failed to set ParentDatasetFile_ID on Original Parent in Dataset Management.");
                        builder.Append($"DatasetFile_ID: {df_Orig.DatasetFileId}");
                        builder.Append($"File_NME: {df_Orig.FileName}");
                        builder.Append($"ParentDatasetFile_ID: {df_Orig.ParentDatasetFileId}");                        
                    }
                }

                Event f = new Event();
                f.EventType = dscontext.EventTypes.Where(w => w.Description == "Created File").FirstOrDefault();
                f.Status = dscontext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                f.TimeCreated = DateTime.Now;
                f.TimeNotified = DateTime.Now;
                f.IsProcessed = false;
                f.UserWhoStartedEvent = response.RequestInitiatorId;
                f.Dataset = response.DatasetID;
                f.DataConfig = response.DatasetFileConfigId;
                f.Reason = $"Successfully Uploaded file [<b>{Path.GetFileName(targetFileName)}</b>] to dataset [<b>{df_newParent.Dataset.DatasetName}</b>]";
                f.Parent_Event = response.RequestGuid;
                Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);

                //return df_newParent;
            }
            else
            {
                throw new NotImplementedException($"The Option of not Overwritting a DataFile is not implemented.  Change OverwriteDataFile_IND setting on Dataset_ID:{dfc.ParentDataset.DatasetId} Config_ID:{dfc.ConfigId} Config_Name:{dfc.Name}");
            }

            if (!isBundled && dfc.CreateCurrentFile)
            {
                Logger.Info("Creating Current File...");

                try
                {
                    //Create target directory if does not exist
                    Directory.CreateDirectory(dfc.GetCurrentFileDir().LocalPath);

                    //Delete contents of current file dir, since there should only be one file
                    // in this location at any given time.
                    foreach(string file in Directory.GetFiles(dfc.GetCurrentFileDir().LocalPath))
                    {
                        File.Delete(file);
                    }

                    Logger.Debug($"Current file target : {Path.Combine(dfc.GetCurrentFileDir().LocalPath, targetFileName)}");

                    //Copy file to current file directory
                    //Using the overwrite option since this should only ever be the latest version
                    File.Copy(fileInfo.FullName, Path.Combine(dfc.GetCurrentFileDir().LocalPath, targetFileName), true);


                    Event f = new Event();
                    f.EventType = dscontext.EventTypes.Where(w => w.Description == "Current File Created").FirstOrDefault();
                    f.Status = dscontext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                    f.TimeCreated = DateTime.Now;
                    f.TimeNotified = DateTime.Now;
                    f.IsProcessed = false;
                    f.UserWhoStartedEvent = response.RequestInitiatorId;
                    f.Dataset = response.DatasetID;
                    f.DataConfig = response.DatasetFileConfigId;
                    f.Reason = $"Successfully created new current file for [<b>{dfc.ParentDataset.DatasetName}</b>] dataset.";
                    f.Parent_Event = response.RequestGuid;
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to create current file", ex);

                    Event f = new Event();
                    f.EventType = dscontext.EventTypes.Where(w => w.Description == "Current File Created").FirstOrDefault();
                    f.Status = dscontext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                    f.TimeCreated = DateTime.Now;
                    f.TimeNotified = DateTime.Now;
                    f.IsProcessed = false;
                    f.UserWhoStartedEvent = response.RequestInitiatorId;
                    f.Dataset = response.DatasetID;
                    f.DataConfig = response.DatasetFileConfigId;
                    f.Reason = $"Failed to created current file for [<b>{dfc.ParentDataset.DatasetName}</b>] dataset.";
                    f.Parent_Event = response.RequestGuid;
                    Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                }
            }
            */
            return df_newParent;
        }
        private static string GetTargetFileName(DatasetFileConfig dfc, string filename)
        {
            string outFileName = null;
            // Are we overwritting target file
            if (dfc.OverwriteDatafile)
            {
                // Non-Regex and TargetFileName is null
                // Use SearchCriteria value
                if (!(dfc.IsRegexSearch) && String.IsNullOrWhiteSpace(dfc.TargetFileName))
                {
                    outFileName = dfc.SearchCriteria;
                }
                // Non-Regex and TargetFileName has value
                // Use TargetFileName value
                else if (!(dfc.IsRegexSearch) && !(String.IsNullOrWhiteSpace(dfc.TargetFileName)))
                {
                    outFileName = dfc.TargetFileName;
                }
                // Regex and TargetFileName has value
                // Use TargetFileName value
                else if (dfc.IsRegexSearch && !(String.IsNullOrWhiteSpace(dfc.TargetFileName)))
                {
                    outFileName = dfc.TargetFileName;
                }
                // Regex and TargetFileName is null - Use input file name
                else if (dfc.IsRegexSearch && String.IsNullOrWhiteSpace(dfc.TargetFileName))
                {
                    outFileName = filename;
                }
            }

            return outFileName;
        }
        private static DatasetFile CreateParentDatasetFile(Dataset ds, DatasetFileConfig dfc, string uploaduser, string targetFileName, DatasetFile CurrentDatasetFile, bool isbundle)
        {
            DatasetFile out_df = null;
            string fileLocation = null;

            string fileOwner = uploaduser;
            DateTime processTime = DateTime.Now;


            if (isbundle)
            {
                StringBuilder location = new StringBuilder();
                location.Append(Configuration.Config.GetHostSetting("S3BundlePrefix"));
                location.Append(GenerateLocationKey(ds.DatasetCategory.Name, ds.DatasetName));
                location.Append(targetFileName);
                fileLocation = location.ToString();
            }
            else
            {
                if (CurrentDatasetFile == null)
                {
                    fileLocation = Utilities.GenerateDatafileKey(ds, processTime, targetFileName, dfc.ConfigId);
                }
                else
                {
                    fileLocation = CurrentDatasetFile.FileLocation;
                }
            }

            out_df = new DatasetFile(
                        0,
                        targetFileName,
                        ds,
                        dfc,
                        fileOwner,
                        fileLocation,
                        processTime,
                        processTime,
                        null,
                        null,
                        isbundle,
                        0,
                        true);


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
                    return true;
                case "txt":
                    return true;
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
            string start = input + DateTime.Now.ToString();
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
        public static async Task CreateEventAsync(Event e)
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {
                //https://stackoverflow.com/questions/16697346/datetime-json-return-from-webapi-with-default-serializer
                var json = JsonConvert.SerializeObject(e, Formatting.None, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Local});     

                HttpContent contentPost = new StringContent(json, Encoding.UTF8,
                    "application/json");

                client.BaseAddress = new Uri(Configuration.Config.GetHostSetting("WebApiUrl"));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync("api/event/create", contentPost);

                if (response.IsSuccessStatusCode)
                {
                    // WHY?>
                }
            }
        }
    }
}

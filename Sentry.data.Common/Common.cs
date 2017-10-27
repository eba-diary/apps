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

namespace Sentry.data.Common
{
    /// <summary>
    /// Provides common code between projects
    /// </summary>
    public static class Utilities
    {
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
        public static string GenerateDatasetDropLocation(string creationFrequency, string categoryName, string datasetName)
        {
            string filep = Path.Combine(Configuration.Config.GetHostSetting("DatasetLoaderBaseLocation"), categoryName.ToLower());
            filep = Path.Combine(filep, datasetName.Replace(' ', '_').ToLower());
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
            return GenerateLocationKey(ds.CreationFreqDesc, ds.DatasetCategory.Name, ds.DatasetName);
        }
        /// <summary>
        /// Generate storage location path.  <i>Specify all parameters</i>
        /// </summary>
        /// <param name="creationFrequency"></param>
        /// <param name="categoryName"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static string GenerateDatasetStorageLocation(string creationFrequency, string categoryName, string datasetName)
        {
            return GenerateLocationKey(creationFrequency, categoryName, datasetName);
        }
        /// <summary>
        /// Generate storage key for datafile
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="now"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GenerateDatafileKey(Dataset ds, DateTime now, string filename)
        {
            StringBuilder location = new StringBuilder();
            location.Append(GenerateDatasetStorageLocation(ds));
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
        public static string GenerateLocationKey(string creationFreqDesc, string category, string datasetName)
        {
            StringBuilder location = new StringBuilder();
            location.Append(Configuration.Config.GetHostSetting("S3DataPrefix"));
            location.Append(category.ToLower());
            location.Append('/');
            location.Append(FormatDatasetName(datasetName));
            location.Append('/');
            location.Append(GenerateDatasetFrequencyLocationName(creationFreqDesc));
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
            List<DatasetFileConfig> filelist = dscontext.getAllDatasetFileConfigs().Where(w => w.DatasetId == datasetId).ToList();
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
        /// <param name="dsService"></param>
        /// <param name="dscontext"></param>
        /// <param name="file"></param>
        /// <param name="uploadUserName"></param>
        /// <returns></returns>
        public static DatasetFile ProcessInputFile(Dataset dataset, DatasetFileConfig dfConfig, IDatasetService dsService, IDatasetContext dscontext, FileInfo file, string uploadUserName)
        {
            return ProcessFile(dataset, dfConfig, dsService, dscontext, file, null, uploadUserName, file.Name);
        }
        /// <summary>
        /// Processes filestream and return DatasetFile object. Transfer events are pushed to OnTransferProgressEvent.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="dfConfig"></param>
        /// <param name="dsService"></param>
        /// <param name="dscontext"></param>
        /// <param name="file"></param>
        /// <param name="uploadUserName"></param>
        /// <returns></returns>
        public static DatasetFile ProcessInputFile(Dataset dataset, DatasetFileConfig dfConfig, IDatasetService dsService, IDatasetContext dscontext, HttpPostedFileBase file, string uploadUserName)
        {
            return ProcessFile(dataset, dfConfig, dsService, dscontext, null, file, uploadUserName, System.IO.Path.GetFileName(file.FileName));
        }
        private static DatasetFile ProcessFile(Dataset ds, DatasetFileConfig dfc, IDatasetService upload, IDatasetContext dscontext, FileInfo fileInfo, HttpPostedFileBase filestream, string uploadUserName, string filename)
        {
            DatasetFile df_Orig = null;
            DatasetFile df_newParent = null;
            string targetFileName = null;
            int df_id = 0;            
            

            // Determine target file name
            targetFileName = GetTargetFileName(dfc, filename);

            if (dfc.OverwriteDatafile)
            {
                // RegexSearch requires passing targetFileName to esnure we get the correct related data file.
                if (dfc.IsRegexSearch)
                {
                    df_id = dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.DatasetId, dfc.DataFileConfigId, targetFileName);
                }
                else
                {
                    df_id = dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.DatasetId, dfc.DataFileConfigId);
                }


                //If datafiles exist for this DatasetFileConfig
                if (df_id != 0)
                {
                    df_Orig = dscontext.GetDatasetFile(df_id);
                    df_newParent = CreateParentDatasetFile(ds, dfc, uploadUserName, targetFileName, df_Orig);
                    //df_updated = updateOverwrittenDatasetFile(ds, df_Orig, fileInfo);
                }
                //If there are no datafiles for this DatasetFileConfig
                else
                {
                    df_newParent = CreateParentDatasetFile(ds, dfc, uploadUserName, targetFileName, null);
                }

                DateTime startUploadTime = DateTime.Now;
                // Upload new key 
                try
                {
                    if (filestream == null)
                    {
                        df_newParent.VersionId = upload.UploadDataset_v2(fileInfo.FullName, df_newParent.FileLocation);
                    }
                    else
                    {    
                        upload.TransferUtlityUploadStream(df_newParent.FileLocation, df_newParent.FileName, filestream.InputStream);
                    }                    
                }
                catch (AmazonS3Exception eS3)
                {
                    Sentry.Common.Logging.Logger.Error("S3 Upload Error", eS3);
                    //SendNotification(datasetMetadata, (int)ExitCodes.S3UploadError, 0, "S3 Upload Error", df_newParent.FileLocation);
                    throw new Exception("S3 Upload Error", eS3);
                    //return (int)ExitCodes.S3UploadError;
                    
                }
                catch (Exception e)
                {
                    Sentry.Common.Logging.Logger.Error("Error during establishing upload process", e);
                    //SendNotification(datasetMetadata, (int)ExitCodes.Failure, 0, "Error during establishing upload process", df_newParent.FileLocation);
                    throw new Exception("Error during establishing upload process", e);
                    //return (int)ExitCodes.Failure;
                }

                //PushFileToStorage(ds, datasetMetadata, upload, dscontext, fileInfo);
                var diffInSeconds = (DateTime.Now - startUploadTime).TotalSeconds;
                Sentry.Common.Logging.Logger.Info($"TransferTime: {diffInSeconds} | DatasetName: {ds.DatasetName}");


                //Register new Parent DatasetFile
                try
                {
                    //Write dataset to database
                    dscontext.Merge(df_newParent);
                    dscontext.SaveChanges();
                }
                catch (Exception e)
                {

                    StringBuilder builder = new StringBuilder();
                    builder.Append("Failed to record new Parent DatasetFile to Dataset Management.");
                    builder.Append($"File_NME: {df_newParent.FileName}");
                    builder.Append($"Dataset_ID: {df_newParent.Dataset.DatasetId}");
                    builder.Append($"UploadUser_NME: {df_newParent.UploadUserName}");
                    builder.Append($"Create_DTM: {df_newParent.CreateDTM}");
                    builder.Append($"Modified_DTM: {df_newParent.ModifiedDTM}");
                    builder.Append($"FileLocation: {df_newParent.FileLocation}");
                    builder.Append($"DataFileConfig_ID: {df_newParent.DatasetFileConfig.DataFileConfigId}");
                    builder.Append($"ParentDatasetFile_ID: {df_newParent.ParentDatasetFileId}");
                    builder.Append($"Version_ID: {df_newParent.VersionId}");

                    Sentry.Common.Logging.Logger.Error(builder.ToString(), e);

                    //SendNotification(datasetMetadata, (int)ExitCodes.DatabaseError, 0, "Error saving dataset to database", df_newParent.FileLocation);
                    throw new Exception("Error saving dataset to database", e);
                    //return (int)ExitCodes.DatabaseError;
                }

                // If there were existing datasetfiles set parentdatasetFile_ID on old parent
                if (df_id != 0)
                {
                    try
                    {
                        //Version the Old Parent DatasetFile
                        int df_newParentId = dscontext.GetLatestDatasetFileIdForDatasetByDatasetFileConfig(dfc.DatasetId, dfc.DataFileConfigId);
                        df_Orig.ParentDatasetFileId = df_newParentId;

                        //Write dataset to database
                        dscontext.Merge(df_Orig);
                        dscontext.SaveChanges();

                    }
                    catch (Exception e)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append("Failed to set ParentDatasetFile_ID on Original Parent in Dataset Management.");
                        builder.Append($"DatasetFile_ID: {df_Orig.DatasetFileId}");
                        builder.Append($"File_NME: {df_Orig.FileName}");
                        builder.Append($"ParentDatasetFile_ID: {df_Orig.ParentDatasetFileId}");                        
                    }
                }
                //SendNotification(datasetMetadata, (int)ExitCodes.Success, ds.DatasetId, string.Empty, string.Empty);

                return df_newParent;
            }

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
        private static DatasetFile CreateParentDatasetFile(Dataset ds, DatasetFileConfig dfc, string uploaduser, string targetFileName, DatasetFile CurrentDatasetFile)
        {
            DatasetFile out_df = null;
            string fileLocation = null;

            string fileOwner = uploaduser;
            DateTime processTime = DateTime.Now;


            if (CurrentDatasetFile == null)
            {
                fileLocation = Utilities.GenerateDatafileKey(ds, processTime, targetFileName);
            }
            else
            {
                fileLocation = CurrentDatasetFile.FileLocation;
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
                        null
                        );


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
        public static Boolean IsExtentionPushToSAScompatible(string extension)
        {
            switch (extension)
            {
                case "csv":
                    return true;
                default:
                    return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.Common.Logging;
using StructureMap;
using Sentry.data.Infrastructure;
using System.IO;
using Sentry.data.Common;
using Newtonsoft.Json;

namespace Sentry.data.Goldeneye
{
    class DatasetLoader
    {

        class SystemConfig
        {
            public string systemName { get; set; }
            public List<FileConfig> fileConfigs { get; set; }
        }

        class FileConfig
        {
            public FileSearch fileSearch { get; set; }
            public DatasetMetadata datasetMetadata { get; set; }
        }

        class FileSearch
        {
            public string fileName { get; set; }
            public string regex { get; set; }
        }

        public static async Task<LoaderRequest> Run(string reqPath)
        {
            LoaderRequest req = null;  
                       
            //Process request object
            try
            {
                //Attempt to deserialize incoming request object
                string incomingRequest = System.IO.File.ReadAllText(reqPath);

                //Get request from DatasetBundler location
                req = JsonConvert.DeserializeObject<LoaderRequest>(incomingRequest);
                if (req == null || req.RequestGuid == "00000000-0000-0000-0000-000000000000")
                {
                    throw new JsonSerializationException($"Failed Deserialzing Request File Path:{reqPath}");
                }

                IContainer container;
                IDatasetContext _datasetContext;

                //Console.WriteLine($"Dataset Loader Connection String:{Configuration.Config.GetHostSetting("DatabaseConnectionString")}");
                using (container = Bootstrapper.Container.GetNestedContainer())
                {
                    _datasetContext = container.GetInstance<IDatasetContext>();
                    string SystemDir = null;
                    string SystemName = null;


                    Logger.Info($"Starting to process RequestGuid:{req.RequestGuid}");

                    try
                    {
                        //Set additional properties for non-bundled requests
                        if (!req.IsBundled)
                        {
                            SystemDir = Directory.GetParent(req.File).FullName + @"\";
                            SystemName = Directory.GetParent(req.File).Name;
                            Logger.Debug($"SystemDir: {SystemDir}");
                            Logger.Debug($"SystemName: {SystemName}");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Loader Request Failed(File Access Error): RequestGuid:{req.RequestGuid}", e);
                    }

                    if (!req.IsBundled)
                    {
                        List<DatasetFileConfig> fileConfigs = new List<DatasetFileConfig>();
                        try
                        {
                            fileConfigs = Utilities.LoadDatasetFileConfigsByDir(SystemDir, _datasetContext);
                            Logger.Debug($"Count of fileConfigs Loaded: {fileConfigs.Count}");
                            SingleFileProcessor(fileConfigs, req.File, _datasetContext, req);

                        }
                        catch (Exception e)
                        {
                            Logger.Error("Error Processing File", e);
                            Event f = new Event();
                            f.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Upload Failure").FirstOrDefault();
                            f.Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                            f.TimeCreated = DateTime.Now;
                            f.TimeNotified = DateTime.Now;
                            f.IsProcessed = false;
                            f.UserWhoStartedEvent = req.RequestInitiatorId;
                            f.Dataset = req.DatasetID;
                            f.DataConfig = req.DatasetFileConfigId;
                            f.Reason = $"Failed uploading file [<b>{Path.GetFileName(req.File)}</b>] to dataset [<b>{_datasetContext.GetById(req.DatasetID).DatasetName}</b>]";
                            f.Parent_Event = req.RequestGuid;
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                        }
                    }
                    else
                    {
                        try
                        {
                            DatasetFileConfig fileconfig = _datasetContext.getDatasetDefaultConfig(req.DatasetID);
                            Dataset ds = _datasetContext.GetById<Dataset>(req.DatasetID);

                            //DatasetFile df = Utilities.ProcessBundleFile(fileconfig, ds, response, dscontext);
                            DatasetFile df = Utilities.ProcessInputFile(ds, fileconfig, _datasetContext, true, req, null);

                            //remove request file
                            Utilities.RemoveProcessedFile(df, new FileInfo(reqPath));

                            //Create Success Event for bundled File Created
                            Event e = new Event();
                            e.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Bundle File Process").FirstOrDefault();
                            e.Status = _datasetContext.EventStatus.Where(w => w.Description == "Success").FirstOrDefault();
                            e.TimeCreated = DateTime.Now;
                            e.TimeNotified = DateTime.Now;
                            e.IsProcessed = false;
                            e.UserWhoStartedEvent = req.RequestInitiatorId;
                            e.Dataset = ds.DatasetId;
                            //e.DataFile = df.DatasetFileId;
                            e.DataConfig = fileconfig.ConfigId;
                            e.Reason = $"Successfully uploaded bundled file [<b>{req.TargetFileName}</b>] to dataset [<b>{_datasetContext.GetById(ds.DatasetId).DatasetName}</b>]";
                            e.Parent_Event = req.RequestGuid;
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                        }
                        catch (Exception e)
                        {
                            Logger.Error("Error Processing File", e);

                            //Create Failure Event for bundled File Created
                            Event f = new Event();

                            f.EventType = _datasetContext.EventTypes.Where(w => w.Description == "Bundle File Process").FirstOrDefault();
                            f.Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault();
                            f.TimeCreated = DateTime.Now;
                            f.TimeNotified = DateTime.Now;
                            f.IsProcessed = false;
                            f.UserWhoStartedEvent = req.RequestInitiatorId;
                            f.Dataset = req.DatasetID;
                            f.DataConfig = req.DatasetFileConfigId;
                            f.Reason = $"Failed processing bundled file";
                            f.Parent_Event = req.RequestGuid;
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);

                            throw new Exception("Error Processing File", e);
                        }
                    }

                    //Remove Dataset loader Request file
                    //when processing requests via SDP this step will no longer be needed
                    try
                    {
                        File.Delete($"{Configuration.Config.GetHostSetting("LoaderRequestPath")}\\{Configuration.Config.GetHostSetting("ProcessedFilePrefix")}{req.RequestGuid}.json");
                    }
                    catch (Exception e)
                    {
                        //Allow application to continue without error.  Log message for BI Portal SOS group.
                        Logger.Error($"Failed deleting Dataset loader request file: RequestGuid:{req.RequestGuid}", e);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is JsonSerializationException)
                {
                    Logger.Error("Loader Request Failed ", ex);
                }
                else
                {
                    Logger.Error($"Loader Request Failed - RequestGuid:{req.RequestGuid}", ex);
                }

                //Move request to failed request directory for potential reprocessing later
                var processFileName = Path.GetFileName(reqPath).Replace(Configuration.Config.GetHostSetting("ProcessedFilePrefix"),"");
                var originalFile = Configuration.Config.GetHostSetting("LoaderFailedRequestPath") + processFileName;                
                File.Move(reqPath, originalFile);

                throw;
            }

            return req;
        }
        

        private static void SingleFileProcessor(List<DatasetFileConfig> systemMetaFiles, string _path, IDatasetContext dscontext, LoaderRequest req = null)
        {
            int configMatch = 0;

            //Add ProcessingPrefix to file name

            //Find matching configs for the given incoming file path
            List<DatasetFileConfig> fcList = Utilities.GetMatchingDatasetFileConfigs(systemMetaFiles, _path);

            FileInfo fi = new FileInfo(_path);

            foreach (DatasetFileConfig fc in fcList.Where(w => w.IsGeneric == false).Take(1))
            {
                Logger.Debug($"Found non-generic DatasetFileConfig: ID-{fc.ConfigId}, Name-{fc.Name}");
                Dataset ds = dscontext.GetById(fc.ParentDataset.DatasetId);
                req.DatasetID = ds.DatasetId;
                req.DatasetFileConfigId = fc.ConfigId;
                DatasetFile df = Utilities.ProcessInputFile(ds, fc, dscontext, false, req, fi);
                Utilities.RemoveProcessedFile(df, new FileInfo(_path));
                configMatch++;
            }

            if (configMatch == 0)
            {
                DatasetFileConfig fc = systemMetaFiles.Where(w => w.IsGeneric == true).FirstOrDefault();
                Logger.Debug($"Using generic DatasetFileConfig: ID-{fc.ConfigId}, Name-{fc.Name}");
                Logger.Debug($"Retrieving Dataset associated with DatasetFileConfig: ID-{fc.ParentDataset.DatasetId}");
                Dataset ds = dscontext.GetById(fc.ParentDataset.DatasetId);
                Logger.Debug("Processing DatasetFile");
                req.DatasetID = ds.DatasetId;
                req.DatasetFileConfigId = fc.ConfigId;
                DatasetFile df = Utilities.ProcessInputFile(ds, fc, dscontext, false, req, fi);

                Logger.Debug($"Removing successful processed file - Path:{_path}");
                Utilities.RemoveProcessedFile(df, new FileInfo(_path));
            }            
        }
    }
}

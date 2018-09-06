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
        public static async Task<LoaderRequest> Run(string reqPath)
        {
            LoaderRequest req = null;

            IContainer container;
            IDatasetContext _datasetContext;
            IRequestContext _requestContext;

            using (container = Bootstrapper.Container.GetNestedContainer())
            {
                _datasetContext = container.GetInstance<IDatasetContext>();
                _requestContext = container.GetInstance<IRequestContext>();

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

                    RetrieverJob job = null;

                    Logger.Info($"Starting to process RequestGuid:{req.RequestGuid}");                    
                    
                    //Set additional properties for non-bundled requests
                    if (!req.IsBundled)
                    {
                        job = _requestContext.RetrieverJob.Where(w => w.Id == req.RetrieverJobId).FirstOrDefault();

                        SingleFileProcessor(req.File, _datasetContext, req, job);
                    }
                    else
                    {
                        //need to determine way to return datafile config
                        DatasetFileConfig fileconfig = null;
                        //DatasetFileConfig fileconfig = _datasetContext.getDatasetDefaultConfig(req.DatasetID);
                        Dataset ds = _datasetContext.GetById<Dataset>(req.DatasetID);
                        
                        DatasetFile df = Utilities.ProcessInputFile(ds, fileconfig, true, req, null);

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
                        e.DataConfig = fileconfig.ConfigId;
                        e.Reason = $"Successfully uploaded bundled file [<b>{req.TargetFileName}</b>] to dataset [<b>{_datasetContext.GetById(ds.DatasetId).DatasetName}</b>]";
                        e.Parent_Event = req.RequestGuid;
                        Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);
                        
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
                    }                
                }
                catch (Exception ex)
                {
                    if (ex is JsonSerializationException)
                    {
                        Logger.Error("Loader Request Serialization Failed ", ex);
                    }
                    else
                    {
                        Logger.Error($"Loader Request Failed - RequestGuid:{req.RequestGuid}", ex);

                        if (!req.IsBundled)
                        {
                            Event f = new Event()
                            {
                                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Upload Failure").FirstOrDefault(),
                                Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault(),
                                TimeCreated = DateTime.Now,
                                TimeNotified = DateTime.Now,
                                IsProcessed = false,
                                UserWhoStartedEvent = req.RequestInitiatorId,
                                Dataset = req.DatasetID,
                                DataConfig = req.DatasetFileConfigId,
                                Reason = $"Failed uploading file [<b>{Path.GetFileName(req.File).Replace(Configuration.Config.GetHostSetting("ProcessedFilePrefix"),"")}</b>] to dataset [<b>{_datasetContext.GetById(req.DatasetID).DatasetName}</b>]",
                                Parent_Event = req.RequestGuid
                            };
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                        }
                        else
                        {
                            //Create Failure Event for bundled File Created
                            Event f = new Event()
                            {
                                EventType = _datasetContext.EventTypes.Where(w => w.Description == "Bundle File Process").FirstOrDefault(),
                                Status = _datasetContext.EventStatus.Where(w => w.Description == "Error").FirstOrDefault(),
                                TimeCreated = DateTime.Now,
                                TimeNotified = DateTime.Now,
                                IsProcessed = false,
                                UserWhoStartedEvent = req.RequestInitiatorId,
                                Dataset = req.DatasetID,
                                DataConfig = req.DatasetFileConfigId,
                                Reason = $"Failed processing bundled file",
                                Parent_Event = req.RequestGuid
                            };
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);
                        }
                    }

                    //Move request to failed request directory for potential reprocessing later
                    var processFileName = Path.GetFileName(reqPath).Replace(Configuration.Config.GetHostSetting("ProcessedFilePrefix"),"");
                    var originalFile = Configuration.Config.GetHostSetting("LoaderFailedRequestPath") + processFileName;                
                    File.Move(reqPath, originalFile);                   

                    throw;
                }
            }

            return req;
        }
        

        private static void SingleFileProcessor(string _path, IDatasetContext dscontext, LoaderRequest req = null, RetrieverJob job = null)
        {
            Logger.Debug($"Processing file for DatasetFileConfig: ID-{job.DatasetConfig.ConfigId}, Name-{job.DatasetConfig.Name}, Job-{job.Id}");
            Dataset ds = dscontext.GetById(job.DatasetConfig.ParentDataset.DatasetId);
            req.DatasetID = job.DatasetConfig.ParentDataset.DatasetId;
            req.DatasetFileConfigId = job.DatasetConfig.ConfigId;
            DatasetFile df = Utilities.ProcessInputFile(ds, job.DatasetConfig, false, req, _path);
            
            //If datasource if DFS delete the incoming file.  S3 data sources is handled within the ProcesinputFile method.
            if (job.DataSource.Is<DfsBasic>() || job.DataSource.Is<DfsCustom>())
            {
                Utilities.RemoveProcessedFile(df, new FileInfo(_path));
            }                          
        }
    }
}

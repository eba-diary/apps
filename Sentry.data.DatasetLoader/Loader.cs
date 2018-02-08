using System;
using System.Collections.Generic;
using Sentry.Common.Logging;
using Sentry.Configuration;
using System.Linq;
using System.Text;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System.IO;
using Newtonsoft.Json;
using StructureMap;
using Amazon.S3;
using System.Net.Mail;
using Sentry.data.DatasetLoader.Entities;
using System.Text.RegularExpressions;
using System.Security.Principal;
using Sentry.data.Web.Helpers;
using Sentry.data.Common;
using System.Web;
using System.Threading.Tasks;

namespace Sentry.data.DatasetLoader
{
    class Loader
    {
        //private IDatasetContext _dscontext;
        //private IContainer _container;
        //private string _file;

        //public Loader(string file, IDatasetContext dscontext)
        //{
        //    _dscontext = dscontext;
        //    _file = file;
        //}

        //public async void Run(string SystemDir, string SystemName, bool isBundled)
        //{
            
        //    try
        //    {
        //        Logger.Info($"Starting to process {_file}");
        //        /////Task.Factory.StartNew(async x =>
        //        ////{
        //        ////Call your bootstrapper to initialize your application
        //        ////Bootstrapper.Init();
        //        //Sentry.data.Infrastructure.Bootstrapper.Init();
        //        ////create an IOC (structuremap) container to wrap this transaction
        //        ////using (container = Bootstrapper.Container.GetNestedContainer)
        //        ////{
        //        ////    var service = container.GetInstance<MyService>();
        //        ////    var result = service.DoWork();
        //        ////    container.GetInstance<ISentry.data.DatasetLoaderContext>.SaveChanges();
        //        ////}



        //        //using (_container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
        //        //{
        //        //_dscontext = _container.GetInstance<IDatasetContext>();

        //        Logger.Debug($"SystemDir: {SystemDir}");
        //        Logger.Debug($"SystemName: {SystemName}");


        //        BundleResponse response;

        //        if (isBundled)
        //        {

        //            string incomingRequest = System.IO.File.ReadAllText(_file);
        //            response = JsonConvert.DeserializeObject<BundleResponse>(incomingRequest);

        //            try
        //            {
        //                DatasetFileConfig fileconfig = _dscontext.getDatasetDefaultConfig(response.DatasetID);
        //                Dataset ds = _dscontext.GetById<Dataset>(response.DatasetID);

        //                //DatasetFile df = Utilities.ProcessBundleFile(fileconfig, ds, response, dscontext);
        //                //DatasetFile df = Utilities.ProcessInputFile(ds, fileconfig, _dscontext, null, true, response, null);

        //                //remove request file
        //                Utilities.RemoveProcessedFile(df, new FileInfo(_file));

        //                //Create Success Event for bundled File Created
        //                Event e = new Event();
        //                e.EventType = _dscontext.GetEventType(1);
        //                e.Status = _dscontext.GetStatus(3);
        //                e.TimeCreated = DateTime.Now;
        //                e.TimeNotified = DateTime.Now;
        //                e.IsProcessed = false;
        //                e.UserWhoStartedEvent = response.RequestInitiatorId;
        //                e.Dataset = ds.DatasetId;
        //                e.DataFile = df.DatasetFileId;
        //                e.DataConfig = fileconfig.ConfigId;
        //                e.Reason = $"{response.RequestGuid} : Bundled File Uploaded Successfully";
        //                e.Parent_Event = response.RequestGuid;
        //                await Utilities.CreateEventAsync(e);
        //            }
        //            catch (Exception e)
        //            {
        //                Logger.Error("Error Processing File", e);

        //                //Create Failure Event for bundled File Created
        //                Event f = new Event();
        //                f.EventType = _dscontext.GetEventType(1);
        //                f.Status = _dscontext.GetStatus(4);
        //                f.TimeCreated = DateTime.Now;
        //                f.TimeNotified = DateTime.Now;
        //                f.IsProcessed = false;
        //                f.UserWhoStartedEvent = response.RequestInitiatorId;
        //                f.Dataset = response.DatasetID;
        //                f.DataConfig = response.DatasetFileConfigId;
        //                f.Reason = $"{response.RequestGuid} : Failed Uploading Bundled File";
        //                f.Parent_Event = response.RequestGuid;
        //                await Utilities.CreateEventAsync(f);
        //            }
        //        }
        //        else
        //        {
        //            List<SystemConfig> systemMetaFiles = new List<SystemConfig>();
        //            List<DatasetFileConfig> fileConfigs = new List<DatasetFileConfig>();
        //            try
        //            {
        //                fileConfigs = Utilities.LoadDatasetFileConfigsByDir(SystemDir, _dscontext);
        //                // systemMetaFiles = LoadSystemConfigFiles(SystemDir);

        //                Logger.Debug($"Count of fileConfigs Loaded: {fileConfigs.Count()}");

        //                SingleFileProcessor(fileConfigs, _file, _dscontext);
        //                // SingleFileProcessor(systemMetaFiles, path, upload, dscontext);

        //            }
        //            catch (Exception e)
        //            {
        //                Logger.Error("Error Processing File", e);
        //            }
        //        }

        //        //}

        //        // Keep the console window open in debug mode.
        //        //Console.WriteLine("Press any key to exit.");
        //        //Console.ReadKey();
        //        Logger.Debug("Loader Task Completed Successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("", ex);
        //    }
        //}

        //private static void SingleFileProcessor(List<DatasetFileConfig> systemMetaFiles, string _path, IDatasetContext dscontext)
        //{
        //    int configMatch = 0;

        //    //Find matching configs for the given incoming file path
        //    List<DatasetFileConfig> fcList = Utilities.GetMatchingDatasetFileConfigs(systemMetaFiles, _path);

        //    FileInfo fi = new FileInfo(_path);

        //    foreach (DatasetFileConfig fc in fcList.Where(w => w.IsGeneric == false))
        //    {

        //        Logger.Debug($"Found non-generic DatasetFileConfig: ID-{fc.ConfigId}, Name-{fc.Name}");

        //        Dataset ds = dscontext.GetById(fc.ParentDataset.DatasetId);

        //        DatasetFile df = Utilities.ProcessInputFile(ds, fc, dscontext, Utilities.GetFileOwner(fi), false, null, fi);

        //        Utilities.RemoveProcessedFile(df, new FileInfo(_path));

        //        configMatch++;

        //        break;
        //    }

        //    if (configMatch == 0)
        //    {
        //        DatasetFileConfig fc = systemMetaFiles.Where(w => w.IsGeneric == true).FirstOrDefault();
        //        Logger.Debug($"Using generic DatasetFileConfig: ID-{fc.ConfigId}, Name-{fc.Name}");
        //        Logger.Debug($"Retrieving Dataset associated with DatasetFileConfig: ID-{fc.ParentDataset.DatasetId}");
        //        Dataset ds = dscontext.GetById(fc.ParentDataset.DatasetId);
        //        Logger.Debug("Processing DatasetFile");
        //        DatasetFile df = Utilities.ProcessInputFile(ds, fc, dscontext, Utilities.GetFileOwner(fi), false, null, fi);

        //        Logger.Debug("Removing successful processed file");
        //        Utilities.RemoveProcessedFile(df, new FileInfo(_path));

        //        //ProcessGeneralFile(upload, dscontext, new FileInfo(_path));
        //        //StringBuilder message = new StringBuilder();
        //        //message.AppendLine("Configuration Not Defined for File");
        //        //message.AppendLine($"Path: {Path.GetFullPath(_path)}");

        //        //Logger.Error(message.ToString());

        //        //SendNotification(null, (int)ExitCodes.Failure, 0, message.ToString(), Path.GetFileName(_path));
        //    }

        //}
    }
}

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
using System.Web.Script.Serialization;

namespace Sentry.data.DatasetLoader
{
    /// <summary>
    /// 
    /// </summary>
    public class Class1
    {
        /// <summary>
        /// 
        /// </summary>
        public static IContainer _container;
        /// <summary>
        /// 
        /// </summary>
        public static IDatasetContext _dscontext;

        static int Main(string[] args)
        {
            Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));

            Logger.Debug("DatasetLoader Started.");
            //Core myCore = new Core();
            //myCore.OnStart(args);

            //Console.WriteLine("Press any key to stop");
            //while (!Console.KeyAvailable)
            //{
            //    System.Threading.Thread.Sleep(10);
            //}

            //myCore.OnStop();

            string _file = null;
            string _path = null;

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-f":
                            i++;
                            if (args.Length <= i) throw new ArgumentException(args[i]);
                            _file = args[i];
                            break;

                        case "-p":
                            i++;
                            if (args.Length <= i) throw new ArgumentException(args[i]);
                            _path = args[i];
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (ArgumentException e)
            {
                Logger.Error("Incorrect Parameter", e);
                //return (int)ExitCodes.Failure;
            }

            string SystemDir = null;
            string SystemName = null;
            string path = null;
            bool isBundled = false;

            path = _path.Replace(@"d:\share", Config.GetHostSetting("FileShare"));

            Logger.Info($"Processing: {path}");

            try
            {
                //determine if the incoming files is a bundled file response object
                if (Directory.GetParent(path).Name == "bundle")
                {
                    SystemName = Directory.GetParent(Directory.GetParent(path).ToString()).Name;
                    SystemDir = Directory.GetParent(Directory.GetParent(path).ToString()).FullName + @"\";
                    isBundled = true;
                }
                else
                {
                    SystemDir = Directory.GetParent(path).FullName + @"\";
                    SystemName = Directory.GetParent(path).Name;
                }

            }
            catch (Exception e)
            {
                Logger.Error("File Access Error", e);
                return (int)ExitCodes.Failure;
            }

            //Call your bootstrapper to initialize your application
            Sentry.data.Infrastructure.Bootstrapper.Init();

            using (_container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
            {
                _dscontext = _container.GetInstance<IDatasetContext>();

                Logger.Debug($"SystemDir: {SystemDir}");
                Logger.Debug($"SystemName: {SystemName}");


                BundleResponse response;

                try
                {
                    if (isBundled)
                    {
                        string incomingRequest = System.IO.File.ReadAllText(path);
                        response = JsonConvert.DeserializeObject<BundleResponse>(incomingRequest);

                        try
                        {
                            DatasetFileConfig fileconfig = _dscontext.getDatasetDefaultConfig(response.DatasetID);
                            Dataset ds = _dscontext.GetById<Dataset>(response.DatasetID);

                            //DatasetFile df = Utilities.ProcessBundleFile(fileconfig, ds, response, dscontext);
                            DatasetFile df = Utilities.ProcessInputFile(ds, fileconfig, _dscontext, null, true, response, null);

                            //remove request file
                            Utilities.RemoveProcessedFile(df, new FileInfo(path));

                            //Create Success Event for bundled File Created
                            Event e = new Event();
                            e.EventType = _dscontext.GetEventType(3);
                            e.Status = _dscontext.GetStatus(3);
                            e.TimeCreated = DateTime.Now;
                            e.TimeNotified = DateTime.Now;
                            e.IsProcessed = false;
                            e.UserWhoStartedEvent = response.RequestInitiatorId;
                            e.Dataset = ds.DatasetId;
                            //e.DataFile = df.DatasetFileId;
                            e.DataConfig = fileconfig.ConfigId;
                            e.Reason = $"{response.RequestGuid} : Bundled File Uploaded Successfully";
                            e.Parent_Event = response.RequestGuid;
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(e), TaskCreationOptions.LongRunning);

                        }
                        catch (Exception e)
                        {
                            Logger.Error("Error Processing File", e);

                            //Create Failure Event for bundled File Created
                            Event f = new Event();
                            f.EventType = _dscontext.GetEventType(3);
                            f.Status = _dscontext.GetStatus(4);
                            f.TimeCreated = DateTime.Now;
                            f.TimeNotified = DateTime.Now;
                            f.IsProcessed = false;
                            f.UserWhoStartedEvent = response.RequestInitiatorId;
                            f.Dataset = response.DatasetID;
                            f.DataConfig = response.DatasetFileConfigId;
                            f.Reason = $"{response.RequestGuid} : Failed Uploading Bundled File";
                            f.Parent_Event = response.RequestGuid;
                            Task.Factory.StartNew(() => Utilities.CreateEventAsync(f), TaskCreationOptions.LongRunning);

                            throw new Exception("Error Processing File", e);
                        }
                    }
                    else
                    {
                        List<SystemConfig> systemMetaFiles = new List<SystemConfig>();
                        List<DatasetFileConfig> fileConfigs = new List<DatasetFileConfig>();
                        try
                        {
                            fileConfigs = Utilities.LoadDatasetFileConfigsByDir(SystemDir, _dscontext);
                            // systemMetaFiles = LoadSystemConfigFiles(SystemDir);

                            Logger.Debug($"Count of fileConfigs Loaded: {fileConfigs.Count()}");

                            SingleFileProcessor(fileConfigs, path, _dscontext);
                            // SingleFileProcessor(systemMetaFiles, path, upload, dscontext);

                        }
                        catch (Exception e)
                        {
                            Logger.Error("Error Processing File", e);
                            throw new Exception("Error Processing File", e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error Processing File", ex);
                    return (int)ExitCodes.Failure;
                }               
            }

            // Keep the console window open in debug mode.
            //Console.WriteLine("Press any key to exit.");
            //Console.ReadKey();
            Logger.Debug("Console App completed successfully.");

            return (int)ExitCodes.Success;
        }

        private static void SingleFileProcessor(List<DatasetFileConfig> systemMetaFiles, string _path, IDatasetContext dscontext)
        {
            int configMatch = 0;

            List<DatasetFileConfig> fcList = Utilities.GetMatchingDatasetFileConfigs(systemMetaFiles, _path);

            FileInfo fi = new FileInfo(_path);

            foreach (DatasetFileConfig fc in fcList.Where(w => w.IsGeneric == false))
            {
                Logger.Debug($"Found non-generic DatasetFileConfig: ID-{fc.ConfigId}, Name-{fc.Name}");

                Dataset ds = dscontext.GetById(fc.ParentDataset.DatasetId);

                DatasetFile df = Utilities.ProcessInputFile(ds, fc, dscontext, Utilities.GetFileOwner(fi), false, null, fi);

                Utilities.RemoveProcessedFile(df, new FileInfo(_path));

                configMatch++;

                break;
            }

            if (configMatch == 0)
            {
                
                DatasetFileConfig fc = systemMetaFiles.Where(w => w.IsGeneric == true).FirstOrDefault();
                if (fc == null) { throw new Exception("Generic Config not found"); }
                Logger.Debug($"Using generic DatasetFileConfig - DatasetFileConfig_ID:{fc.ConfigId} Name:{fc.Name}");
                Logger.Debug($"Retrieving Dataset - Dataset_ID:{fc.ParentDataset.DatasetId}");
                Dataset ds = dscontext.GetById(fc.ParentDataset.DatasetId);
                Logger.Debug("Processing DatasetFile");
                DatasetFile df = Utilities.ProcessInputFile(ds, fc, dscontext, Utilities.GetFileOwner(fi), false, null, fi);
                Logger.Debug("Removing successful processed file");
                Utilities.RemoveProcessedFile(df, new FileInfo(_path));
            }
        }
    }
}

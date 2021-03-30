using Sentry.Common.Logging;
using Sentry.data.Core;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public static class WallEService
    {
        private static Guid _runGuid;

        public static async Task Run()
        {
            _runGuid = Guid.NewGuid();
            Logger.Info($"walleservice-run-initiated - guid:{_runGuid}");
            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Factory.StartNew(() => { DeleteSchemas(); }));
            tasks.Add(Task.Factory.StartNew(() => { DeleteDatasets(); }));

                await Task.WhenAll(tasks);

            Logger.Info($"walleservice-run-completed - guid:{_runGuid}");
        }

        private static void DeleteSchemas()
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _datasetContext = container.GetInstance<IDatasetContext>();
                IConfigService configService = container.GetInstance<IConfigService>();
                
                /*  Return list which meet the following:
                 *  1.) Parent dataset is Active
                 *  2.) Config objectstatus == Pending_Delete
                 *  3.) DeleteIssueDTM meets the SchemaDeleteWaitDays configuration
                 *  
                 *  Save the list to the following Tuple format
                 *  Dataset_Id, Dataset_NME, Config_ID, Schema_NME
                 */
                List<Tuple<int, string, int, string>> tupleList  = _datasetContext.DatasetFileConfigs.Where(w => w.ParentDataset.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Active &&
                                                                                                    w.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Pending_Delete &&
                                                                                                    w.DeleteIssueDTM < DateTime.Now.AddDays(Double.Parse(Configuration.Config.GetHostSetting("SchemaDeleteWaitDays")))
                                                                                                ).Select(s => new { s.ParentDataset.DatasetId, s.ParentDataset.DatasetName, s.ConfigId, s.Schema.Name })
                                                                                                .AsEnumerable()
                                                                                                .Select(c => new Tuple<int, string, int, string>(c.DatasetId, c.DatasetName, c.ConfigId, c.Name)).ToList();

                if (tupleList != null && tupleList.Count > 0)
                {
                    Logger.Info($"walleservice-schemadeletes-detected - {tupleList.Count} schemas found - guid:{_runGuid}");
                    foreach (Tuple<int, string, int, string> listItem in tupleList)
                    {
                        Logger.Info($"walleservice-schemadelete-start - DatasetId:{listItem.Item1} DatasetName:{listItem.Item2} ConfigId:{listItem.Item3} ConfigName:{listItem.Item4} guid:{_runGuid}");
                        bool IsSuccessful = configService.Delete(listItem.Item3, false, false);
                        if (!IsSuccessful)
                        {
                            Logger.Info($"walleservice-schemadelete ended with failures - DatasetId:{listItem.Item1} DatasetName:{listItem.Item2} ConfigId:{listItem.Item3} ConfigName:{listItem.Item4} guid:{_runGuid}");
                        }
                        else
                        {
                            Logger.Info($"walleservice-schemadelete-end -DatasetId:{listItem.Item1} DatasetName:{listItem.Item2} ConfigId:{listItem.Item3} ConfigName:{listItem.Item4} guid:{_runGuid}");
                        }
                    }
                }
                else
                {
                    Logger.Info($"walleservice-schemadeletes-notdetected - guid:{_runGuid}");
                }
            }            
        }

        private static void DeleteDatasets()
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _datasetContext = container.GetInstance<IDatasetContext>();
                IDatasetService _datasetService = container.GetInstance<IDatasetService>();
                Dictionary<int, string> dsList = _datasetContext.Datasets.Where(w => w.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Pending_Delete
                                                                                && w.DeleteIssueDTM < DateTime.Now.AddDays(Double.Parse(Configuration.Config.GetHostSetting("DatasetDeleteWaitDays"))))
                                                                                .Select(s => new { s.DatasetId, s.DatasetName })
                                                                                .ToDictionary(d => d.DatasetId, d => d.DatasetName);

                if (dsList != null && dsList.Count > 0)
                {
                    Logger.Info($"walleservice-datasetdeletes-detected - {dsList.Count} datasets found - guid:{_runGuid}");
                    foreach (var ds in dsList)
                    {
                        Logger.Info($"walleservice-datasetdelete-start - DatasetId:{ds.Key} DatasetName:{ds.Value} guid:{_runGuid}");
                        bool IsSuccessful = _datasetService.Delete(ds.Key, false);
                        if (IsSuccessful)
                        {
                            Logger.Info($"walleservice-datasetdelete-end - DatasetId:{ds.Key} DatasetName:{ds.Value} guid:{_runGuid}");
                        }
                        else
                        {
                            Logger.Warn($"walleservice-datasetdelete ended with failures - DatasetId:{ds.Key} DatasetName:{ds.Value} guid:{_runGuid}");
                        }                        
                    }
                }
                else
                {
                    Logger.Info($"walleservice-datasetdeletes-notdetected - guid:{_runGuid}");
                }
            }
        }
    }
}

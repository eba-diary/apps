using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.Common.Logging;

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
                IConfigService configService = container.GetInstance<IConfigService>();
                List<DatasetFileConfig> DeleteSchemaList = configService.GetSchemaMarkedDeleted();

                if (DeleteSchemaList != null && DeleteSchemaList.Count > 0)
                {
                    Logger.Info($"walleservice-schemadeletes-detected - {DeleteSchemaList.Count} schemas found - guid:{_runGuid}");
                    foreach (DatasetFileConfig config in DeleteSchemaList)
                    {
                        Logger.Info($"walleservice-schemadelete-start - DatasetId:{config.ParentDataset.DatasetId} DatasetName:{config.ParentDataset.DatasetName} ConfigId:{config.ConfigId} ConfigName:{config.Name} guid:{_runGuid}");
                        bool IsSuccessful = configService.Delete(config.ConfigId, false, false);
                        if (!IsSuccessful)
                        {
                            Logger.Info($"walleservice-schemadelete ended with failures - DatasetId:{config.ParentDataset.DatasetId} DatasetName:{config.ParentDataset.DatasetName} ConfigId:{config.ConfigId} ConfigName:{config.Name} guid:{_runGuid}");
                        }
                        else
                        {
                            Logger.Info($"walleservice-schemadelete-end - DatasetId:{config.ParentDataset.DatasetId} DatasetName:{config.ParentDataset.DatasetName} ConfigId:{config.ConfigId} ConfigName:{config.Name} guid:{_runGuid}");
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
                IDatasetService datasetService = container.GetInstance<IDatasetService>();
                List<Dataset> deleteDatasetList = datasetService.GetDatasetMarkedDeleted();

                if (deleteDatasetList != null && deleteDatasetList.Count > 0)
                {
                    Logger.Info($"walleservice-datasetdeletes-detected - {deleteDatasetList.Count} datasets found - guid:{_runGuid}");
                    foreach (var ds in deleteDatasetList)
                    {
                        Logger.Info($"walleservice-datasetdelete-start - DatasetId:{ds.DatasetId} DatasetName:{ds.DatasetName} guid:{_runGuid}");
                        bool IsSuccessful = datasetService.Delete(ds.DatasetId, false);
                        if (!IsSuccessful)
                        {
                            Logger.Info($"walleservice-datasetdelete-end - DatasetId:{ds.DatasetId} DatasetName:{ds.DatasetName} guid:{_runGuid}");
                        }
                        else
                        {
                            Logger.Warn($"walleservice-datasetdelete ended with failures - DatasetId:{ds.DatasetId} DatasetName:{ds.DatasetName} guid:{_runGuid}");
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

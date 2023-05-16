using Microsoft.Extensions.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class WallEService
    {
        private Guid _runGuid;

        private readonly IDatasetContext _datasetContext;
        private readonly IDataApplicationService _dataApplicationService;
        private readonly ILogger<WallEService> _logger;

        public WallEService(IDatasetContext datasetContext, IDataApplicationService dataApplicationService, ILogger<WallEService> logger)
        {
            _datasetContext = datasetContext;
            _dataApplicationService = dataApplicationService;
            _logger = logger;

        }

        public async Task Run()
        {
            _runGuid = Guid.NewGuid();
            _logger.LogInformation($"walleservice-run-initiated - guid:{_runGuid}");
            List<Task> tasks = new List<Task>();

            tasks.Add(Task.Factory.StartNew(() => { DeleteSchemas(); }));
            tasks.Add(Task.Factory.StartNew(() => { DeleteDatasets(); }));

                await Task.WhenAll(tasks);

            _logger.LogInformation($"walleservice-run-completed - guid:{_runGuid}");
        }

        private void DeleteSchemas()
        {                
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
                _logger.LogInformation($"walleservice-schemadeletes-detected - {tupleList.Count} schemas found - guid:{_runGuid}");
                foreach (Tuple<int, string, int, string> listItem in tupleList)
                {
                    _logger.LogInformation($"walleservice-schemadelete-start - DatasetId:{listItem.Item1} DatasetName:{listItem.Item2} ConfigId:{listItem.Item3} ConfigName:{listItem.Item4} guid:{_runGuid}");
                    bool IsSuccessful = _dataApplicationService.DeleteDatasetFileConfig(new List<int>() { listItem.Item3 }, null, true);
                    if (!IsSuccessful)
                    {
                        _logger.LogInformation($"walleservice-schemadelete ended with failures - DatasetId:{listItem.Item1} DatasetName:{listItem.Item2} ConfigId:{listItem.Item3} ConfigName:{listItem.Item4} guid:{_runGuid}");
                    }
                    else
                    {
                        _logger.LogInformation($"walleservice-schemadelete-end -DatasetId:{listItem.Item1} DatasetName:{listItem.Item2} ConfigId:{listItem.Item3} ConfigName:{listItem.Item4} guid:{_runGuid}");
                    }
                }
            }
            else
            {
                _logger.LogInformation($"walleservice-schemadeletes-notdetected - guid:{_runGuid}");
            }            
        }

        private void DeleteDatasets()
        {
            Dictionary<int, string> dsList = _datasetContext.Datasets.Where(w => w.ObjectStatus == Core.GlobalEnums.ObjectStatusEnum.Pending_Delete
                                                                            && w.DeleteIssueDTM < DateTime.Now.AddDays(Double.Parse(Configuration.Config.GetHostSetting("DatasetDeleteWaitDays"))))
                                                                            .Select(s => new { s.DatasetId, s.DatasetName })
                                                                            .ToDictionary(d => d.DatasetId, d => d.DatasetName);

            if (dsList != null && dsList.Count > 0)
            {
                _logger.LogInformation($"walleservice-datasetdeletes-detected - {dsList.Count} datasets found - guid:{_runGuid}");
                foreach (var ds in dsList)
                {
                    _logger.LogInformation($"walleservice-datasetdelete-start - DatasetId:{ds.Key} DatasetName:{ds.Value} guid:{_runGuid}");
                    bool IsSuccessful = _dataApplicationService.DeleteDataset(new List<int>(){ ds.Key }, null, true);
                    if (IsSuccessful)
                    {
                        _logger.LogInformation($"walleservice-datasetdelete-end - DatasetId:{ds.Key} DatasetName:{ds.Value} guid:{_runGuid}");
                    }
                    else
                    {
                        _logger.LogWarning($"walleservice-datasetdelete ended with failures - DatasetId:{ds.Key} DatasetName:{ds.Value} guid:{_runGuid}");
                    }                        
                }
            }
            else
            {
                _logger.LogInformation($"walleservice-datasetdeletes-notdetected - guid:{_runGuid}");
            }
        }
    }
}

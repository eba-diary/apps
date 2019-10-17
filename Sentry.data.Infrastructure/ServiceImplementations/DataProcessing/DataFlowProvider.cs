using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.S3;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public class DataFlowProvider : IDataFlowProvider
    {
        public async Task ExecuteDependenciesAsync(S3ObjectEvent s3e)
        {
            await ExecuteDependenciesAsync(s3e.s3.bucket.name, s3e.s3._object.key);
            //await ExecuteDependenciesAsync("sentry-dataset-management-np-nr", "data/17/TestFile.csv");
        }
        public async Task ExecuteDependenciesAsync(string bucket, string key)
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext dsContext = container.GetInstance<IDatasetContext>();
                IDataFlowService dfService = container.GetInstance<IDataFlowService>();
                IMessagePublisher messagePublisher = container.GetInstance<IMessagePublisher>();

                Logger.Info($"start-method <executedependencies>");

                //Get prefix
                string stepPrefix = GetDataFlowStepPrefix(key);

                if (stepPrefix != null)
                {
                    //Find Dependencies
                    List<DataFlowStep> stepList = dsContext.DataFlowStep.Where(w => w.TriggerKey == stepPrefix).ToList();
                    
                    //Generate Start Events
                    foreach (DataFlowStep step in stepList)
                    {
                        IDataFlowStepProvider provider = null;
                        switch (step.DataAction_Type_Id)
                        {
                            case Core.Entities.DataProcessing.DataActionType.S3Drop:
                                provider = container.GetInstance<S3DropProvider>();
                                break;
                            case Core.Entities.DataProcessing.DataActionType.RawStorage:
                                provider = container.GetInstance<RawStorageProvider>();
                                break;
                            case Core.Entities.DataProcessing.DataActionType.QueryStorage:
                                provider = container.GetInstance<QueryStorageProvider>();
                                break;
                            case Core.Entities.DataProcessing.DataActionType.SchemaLoad:
                                provider = container.GetInstance<SchemaLoadProvider>();
                                break;
                            case Core.Entities.DataProcessing.DataActionType.ConvertParquet:
                                provider = container.GetInstance<ConvertToParquetProvider>();
                                break;
                            case Core.Entities.DataProcessing.DataActionType.None:
                            default:
                                throw new NotImplementedException();
                        }

                        string flowExecutionGuid = GetFlowGuid(key);

                        provider.GenerateStartEvent(step, bucket, key, flowExecutionGuid);
                    }
                    Logger.Info($"end-method <executedependencies>");
                }
                else
                {
                    Logger.Info($"executedependencies - invalidstepprefix bucket: {bucket} key:{key}");
                    Logger.Info($"end-method <executedependencies>");
                }
            }
        }

        #region Private Methods

        protected string GetDataFlowStepPrefix(string key)
        {
            Logger.Info($"start-method <getdataflowstepprefix>");

            string filePrefix = null;
            //three level prefixes - temp locations
            // example -  <temp-file prefix>/<step prefix>/<data flow id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX) || 
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX)
               )
            {
                int idx = GetNthIndex(key, '/', 3);
                filePrefix = key.Substring(0, (idx + 1));
            }
            //two level prefixes - non-temp locations
            // example -  <rawstorage prefix>/<job Id>/
            if (filePrefix == null && key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.CONVERT_TO_PARQUET_PREFIX)
               )
            {
                int idx = GetNthIndex(key, '/', 2);
                filePrefix = key.Substring(0, (idx + 1));
            }

            //For drop location prefixes
            // single level prefix
            // example = <data flow id>/
            if (filePrefix == null)
            {
                int idx = GetNthIndex(key, '/', 1);
                bool IsInt = int.TryParse(key.Substring(0, (idx)), out int jobId);
                bool validFlowId = false;
                if (IsInt)
                {                    
                    using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                    {
                        IDatasetContext dsContext = container.GetInstance<IDatasetContext>();
                        validFlowId = dsContext.DataFlow.Any(a => a.Id == jobId);
                    }
                }

                if (validFlowId)
                {
                    filePrefix = key.Substring(0, (idx + 1));
                }                
            }
            Logger.Info($"end-method <getdataflowstepprefix>");

            return filePrefix;            
        }

        private string GetFlowGuid(string key)
        {
            Logger.Info($"start-method <getflowguid>");

            string guidPrefix = null;

            //three level prefixes - temp locations
            // example -  <temp-file prefix>/<step prefix>/<data flow id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX) || 
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX) ||
                key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX)
               )
            {
                int strtIdx = GetNthIndex(key, '/', 3);
                int endIdx = GetNthIndex(key, '/', 4);
                guidPrefix = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            //two level prefixes - non-temp locations
            // example -  <rawstorage prefix>/<job Id>/
            if (guidPrefix == null && key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX))
            {
                int strtIdx = GetNthIndex(key, '/', 2);
                int endIdx = GetNthIndex(key, '/', 3);
                guidPrefix = key.Substring(strtIdx + 1, (endIdx - strtIdx) - 1);
            }

            Logger.Info($"end-method <getflowguid>");
            return guidPrefix;
        }


        private int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        #endregion
    }
}

using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public class DataFlowProvider
    {
        public void ExecuteDependencies(string bucket, string key)
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext dsContext = container.GetInstance<IDatasetContext>();
                IDataFlowService dfService = container.GetInstance<IDataFlowService>();
                IMessagePublisher messagePublisher = container.GetInstance<IMessagePublisher>();

                Logger.Info($"start-method <executedependencies>");

                //Get prefix
                string filePrefix = GetDataFlowStepPrefix(key);

                //Find Dependencies
                List<DataFlowStep> stepList = dsContext.DataFlowStep.Where(w => w.TriggerKey == filePrefix).ToList();

                //Generate Start Events
                foreach(DataFlowStep step in stepList)
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
                    provider.GenerateStartEvent(step, bucket, key);
                }
            }

            Logger.Info($"end-method <executedependencies>");
        }

        #region Private Methods

        protected string GetDataFlowStepPrefix(string key)
        {
            Logger.Info($"start-method <getdataflowstepprefix>");

            string filePrefix = null;
            //three level prefixes
            // example -  <temp-file prefix>/<step prefix>/<data flow id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.S3_DROP_PREFIX))
            {
                int idx = GetNthIndex(key, '/', 3);
                filePrefix = key.Substring(0, (idx + 1));
            }
            //two level prefixes
            // example -  <rawstorage prefix>/<job Id>/
            if (key.StartsWith(GlobalConstants.DataFlowTargetPrefixes.RAW_STORAGE_PREFIX))
            {
                int idx = GetNthIndex(key, '/', 2);
                filePrefix = key.Substring(0, (idx + 1));
            }
            Logger.Info($"start-method <getdataflowstepprefix>");

            return filePrefix;            
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

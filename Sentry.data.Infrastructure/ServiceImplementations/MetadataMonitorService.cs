using Sentry.Common.Logging;
using Sentry.data.Core;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class MetadataMonitorService : IMetadataMonitorService
    {
        public void CheckConsumptionLayerStatus()
        {
            using (IContainer Container = Bootstrapper.Container.GetNestedContainer())
            {
                IDatasetContext _datasetContext = Container.GetInstance<IDatasetContext>();
                IEmailService _emailService = Container.GetInstance<IEmailService>();

                var schemas = _datasetContext.FileSchema.Where(s => !s.DeleteInd && s.Revisions.Any()).ToList();

                List<SchemaConsumptionSnowflake> staleLayers = new List<SchemaConsumptionSnowflake>();

                foreach (var schema in schemas)
                {
                    foreach(var snowflakeConsumptionLayer in schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().Where(l => !(l.SnowflakeStatus == ConsumptionLayerTableStatusEnum.Available.ToString() || l.SnowflakeStatus == ConsumptionLayerTableStatusEnum.Deleted.ToString())).ToList())
                    {
                        if((DateTime.Now - snowflakeConsumptionLayer.LastChanged).TotalHours > 24)
                        {
                            staleLayers.Add(snowflakeConsumptionLayer);
                        }
                    }
                }

                if(staleLayers.Count > 0)
                {
                    _emailService.SendConsumptionLayerStaleEmail(staleLayers);
                }
            }
        }
    }
}

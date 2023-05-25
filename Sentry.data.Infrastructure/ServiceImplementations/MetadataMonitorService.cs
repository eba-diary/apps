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
        public MetadataMonitorService(IDatasetContext datasetContext, IEmailService emailService)
        {
            _datasetContext = datasetContext;
            _emailService = emailService;
        }

        private IDatasetContext _datasetContext;
        private IEmailService _emailService;

        public void CheckConsumptionLayerStatus()
        {
            var schemas = _datasetContext.FileSchema.Where(s => !s.DeleteInd && s.Revisions.Any()).ToList();

            List<SchemaConsumptionSnowflake> staleLayers = new List<SchemaConsumptionSnowflake>();

            foreach (var schema in schemas)
            {
                foreach (var snowflakeConsumptionLayer in schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().Where(l => !(l.SnowflakeStatus == ConsumptionLayerTableStatusEnum.Available.ToString() || l.SnowflakeStatus == ConsumptionLayerTableStatusEnum.Deleted.ToString())).ToList())
                {
                    if ((DateTime.Now - snowflakeConsumptionLayer.LastChanged).TotalHours > 24)
                    {
                        staleLayers.Add(snowflakeConsumptionLayer);
                    }
                }
            }

            if (staleLayers.Count > 0)
            {
                _emailService.SendConsumptionLayerStaleEmail(staleLayers);
            }
        }
    }
}

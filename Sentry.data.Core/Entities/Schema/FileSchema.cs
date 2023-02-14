using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class FileSchema : Schema
    {
        public FileSchema() : base() { }
        public FileSchema(DatasetFileConfig config,IApplicationUser user) : base(config, user)
        {
            Extension = config.FileExtension;
        }
        public virtual FileExtension Extension { get; set; }
        public virtual string Delimiter { get; set; }
        public virtual bool HasHeader { get; set; }
        public virtual bool CreateCurrentView { get; set; }
        public virtual string SasLibrary { get; set; }
        public virtual string HiveTable { get; set; }
        public virtual string HiveDatabase { get; set; }
        public virtual string HiveLocation { get; set; }
        public virtual string HiveTableStatus { get; set; }
        public virtual string StorageCode { get; set; }
        public virtual string SchemaRootPath { get; set; }
        public virtual string ParquetStorageBucket { get; set; }
        public virtual string ParquetStoragePrefix { get; set; }

        public virtual IList<SchemaConsumption> ConsumptionDetails { get; set; }

        public virtual void AddOrUpdateSnowflakeConsumptionLayer(SchemaConsumptionSnowflake newConsumptionLayerMetadata)
        {
            if (ConsumptionDetails == null)
            {
                ConsumptionDetails = new List<SchemaConsumption>();
            }

            //Add consumption layer metadata if it does not exist
            if (!ConsumptionDetails.Cast<SchemaConsumptionSnowflake>().Any(c => c.SnowflakeType == newConsumptionLayerMetadata.SnowflakeType))
            {
                ConsumptionDetails.Add(newConsumptionLayerMetadata);
            }
            //Update existing consumption layer metadata if it exists
            else
            {
                SchemaConsumptionSnowflake originalConsumptionItem = ConsumptionDetails.Cast<SchemaConsumptionSnowflake>().FirstOrDefault(w => w.SnowflakeType == newConsumptionLayerMetadata.SnowflakeType);

                if (originalConsumptionItem != null)
                {
                    originalConsumptionItem.SnowflakeWarehouse = newConsumptionLayerMetadata.SnowflakeWarehouse;
                    originalConsumptionItem.SnowflakeStage = newConsumptionLayerMetadata.SnowflakeStage;
                    originalConsumptionItem.SnowflakeDatabase = newConsumptionLayerMetadata.SnowflakeDatabase;
                    originalConsumptionItem.SnowflakeSchema = newConsumptionLayerMetadata.SnowflakeSchema;
                    originalConsumptionItem.SnowflakeTable = newConsumptionLayerMetadata.SnowflakeTable;
                }
            }
        }        
    }
}

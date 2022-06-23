using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetFileConfigDto
    {
        public virtual int ConfigId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int FileTypeId { get; set; }
        public virtual int ParentDatasetId { get; set; }
        public virtual int DatasetScopeTypeId { get; set; }
        public virtual string StorageCode { get; set; }
        public string StorageLocation { get; set; }
        public virtual int FileExtensionId { get; set; }
        public virtual IList<DataElementDto> Schemas { get; set; }
        public UserSecurity Security { get; internal set; }
        public bool CreateCurrentView { get; set; }
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
        public bool IsTrackableSchema { get; set; }
        public FileSchemaDto Schema { get; set; }
        public int SchemaId { get; set; }
        public string FileExtensionName { get; set; }
        public string HiveTable { get; set; }
        public string HiveDatabase { get; set; }
        public string HiveLocation { get; set; }
        public string HiveTableStatus { get; set; }
        public bool DeleteInd { get; set; }
        public string DeleteIssuer { get; set; }
        public DateTime? DeleteIssueDTM { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }
        public string SchemaRootPath { get; set; }
        public bool HasDataFlow { get; set; }
        public string ParquetStorageBucket { get; set; }
        public string ParquetStoragePrefix { get; set; }
        public IList<SchemaConsumptionDto> ConsumptionDetails { get; set; }
    }
}

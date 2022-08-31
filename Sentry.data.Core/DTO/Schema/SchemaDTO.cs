using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Core
{
    public abstract class SchemaDto
    {
        public int SchemaId { get; set; }
        public abstract string SchemaEntity_NME { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ParentDatasetId { get; set; }
        public virtual ObjectStatusEnum ObjectStatus { get; set; }
        public virtual bool DeleteInd { get; set; }
        public virtual string DeleteIssuer { get; set; }
        public virtual DateTime DeleteIssueDTM { get; set; }
        public virtual bool CLA1396_NewEtlColumns { get; set; }
        public virtual bool CLA1580_StructureHive { get; set; }
        public virtual bool CLA2472_EMRSend { get; set; }
        public virtual bool CLA1286_KafkaFlag { get; set; }
        public virtual bool CLA3014_LoadDataToSnowflake { get; set; }
        public virtual string ControlMTriggerName { get; set; }
    }
}

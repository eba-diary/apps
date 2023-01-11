using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public abstract class Schema
    {
        protected Schema()
        {
            CreatedDTM = DateTime.Now;
            LastUpdatedDTM = DateTime.Now;
        }
        protected Schema(DatasetFileConfig config, IApplicationUser user)
        {
            Name = config.Name;
            CreatedDTM = DateTime.Now;
            LastUpdatedDTM = DateTime.Now;
            CreatedBy = user.AssociateId;
        }
        public virtual int SchemaId { get; set; }
        public virtual string SchemaEntity_NME { get; set; }
        public virtual string Name { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual DateTime LastUpdatedDTM { get; set; }
        public virtual string UpdatedBy { get; set; }
        public virtual string Description { get; set; }
        public virtual ObjectStatusEnum ObjectStatus { get; set; }
        public virtual bool DeleteInd { get; set; }
        public virtual string DeleteIssuer { get; set; }
        public virtual DateTime DeleteIssueDTM { get; set; }

        public virtual IList<SchemaRevision> Revisions { get; set; }

        #region SchemaLevelFeatureFlags
        //These feature flags are at a schema level instead of
        //  an application level
        /// <summary>
        /// These feature
        /// </summary>
        public virtual bool CLA1396_NewEtlColumns { get; set; }
        /// <summary>
        /// This feature triggers the creation of JSON structures within Parquet
        /// </summary>
        public virtual bool CLA1580_StructureHive { get; set; }

        public virtual bool CLA2472_EMRSend { get; set; }
        public virtual bool CLA2429_SnowflakeCreateTable { get; set; }
        public virtual bool CLA1286_KafkaFlag { get; set; }
        public virtual bool CLA3014_LoadDataToSnowflake { get; set; }
        public virtual string ControlMTriggerName { get; set; }

        #endregion

        protected internal virtual void AddRevision(SchemaRevision revision)
        {
            if (Revisions == null)
            {
                Revisions = new List<SchemaRevision>();
            }
            revision.Revision_NBR = (Revisions.Any()) ? Revisions.Count + 1 : 1;
            revision.ParentSchema = this as FileSchema;
            Revisions.Add(revision);
        }
    }
}

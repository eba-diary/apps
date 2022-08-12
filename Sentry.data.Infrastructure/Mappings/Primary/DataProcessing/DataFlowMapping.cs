using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataFlowMapping : ClassMapping<DataFlow>
    {
        public DataFlowMapping()
        {
            this.Table("DataFlow");

            this.Id(x => x.Id, m =>
            {
                m.Column("Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Name, m => m.Column("Name"));
            this.Property(x => x.FlowGuid, m => m.Column("FlowGuid"));
            this.Property(x => x.SaidKeyCode, m => m.Column("SaidKeyCode"));
            this.Property(x => x.DatasetId, m => m.Column("DatasetId"));
            this.Property(x => x.SchemaId, m => m.Column("SchemaId"));
            this.Property(x => x.CreatedDTM, m => m.Column("Create_DTM"));
            this.Property(x => x.CreatedBy, m => m.Column("CreatedBy"));
            this.Property(x => x.Questionnaire, m => m.Column("Questionnaire"));
            this.Property(x => x.FlowStorageCode, m => m.Column("FlowStorageCode"));
            this.Property(x => x.ObjectStatus, m => m.Column("ObjectStatus"));
            this.Property(x => x.DeleteIssuer, m => m.Column("DeleteIssuer"));
            this.Property(x => x.DeleteIssueDTM, m => m.Column("DeleteIssueDTM"));
            this.Property(x => x.IngestionType, m => m.Column("IngestionType"));
            this.Property(x => x.IsDecompressionRequired, m => m.Column("IsDecompressionRequired"));
            this.Property(x => x.CompressionType, m => m.Column("CompressionType"));
            this.Property(x => x.IsPreProcessingRequired, m => m.Column("IsPreProcessingRequired"));
            this.Property(x => x.PreProcessingOption, m => m.Column("PreProcessingOption"));
            
            this.Property(x => x.UserDropLocationBucket);
            this.Property(x => x.UserDropLocationPrefix);
            this.Property(x => x.NamedEnvironment);
            this.Property(x => x.NamedEnvironmentType, attr => attr.Type<EnumStringType<NamedEnvironmentType>>());

            this.Bag(x => x.Steps, (m) =>
            {
                m.Inverse(true);
                m.Table("DataFlowStep");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("DataFlow_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataFlowStep))));

            this.Bag(x => x.Logs, (m) =>
            {
                m.Inverse(true);
                m.Table("DataFlowLog");
                m.Cascade(Cascade.All);
                m.Key((k) =>
                {
                    k.Column("DataFlow_Id");
                });
            }, map => map.OneToMany(a => a.Class(typeof(EventMetric))));

            //ISecurable Mapping
            this.Property((x) => x.IsSecured, (m) => m.Column("IsSecured_IND"));
            this.Property((x) => x.PrimaryContactId, (m) => m.Column("PrimaryContact_ID"));
            this.ManyToOne(x => x.Security, m =>
            {
                m.Column("Security_ID");
                m.ForeignKey("FK_Dataset_Security");
                m.Class(typeof(Security));
                m.Cascade(Cascade.All);
            });
        }
    }
}

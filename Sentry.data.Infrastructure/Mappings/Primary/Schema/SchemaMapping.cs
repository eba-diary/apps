using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SchemaMapping : ClassMapping<Schema>
    {
        public SchemaMapping()
        {
            Table("[Schema]");

            Id(x => x.SchemaId, m =>
            {
                m.Column("Schema_Id");
                m.Generator(Generators.Identity);
            });

            Property((x) => x.Name, (m) => m.Column("Schema_NME"));
            Discriminator(x => x.Column("SchemaEntity_NME"));
            Property(x => x.CreatedBy, m => m.Column("CreatedBy"));
            Property(x => x.CreatedDTM, m => m.Column("Created_DTM"));
            Property(x => x.LastUpdatedDTM, m => m.Column("LastUpdatd_DTM"));
            Property(x => x.Description, m => m.Column("Description"));
            Property(x => x.ObjectStatus, m => m.Column("ObjectStatus"));
            Property(x => x.DeleteInd, m => m.Column("DeleteInd"));
            Property(x => x.DeleteIssuer, m => m.Column("DeleteIssuer"));
            Property(x => x.DeleteIssueDTM, m => m.Column("DeleteIssueDTM"));
            Property(x => x.CLA1396_NewEtlColumns, m => m.Column("CLA1396_NewEtlColumns"));
            Property(x => x.CLA1580_StructureHive, m => m.Column("CLA1580_StructureHive"));
            Property(x => x.CLA2472_EMRSend, m => m.Column("CLA2472_EMRSend"));
            Property(x => x.CLA1286_KafkaFlag, m => m.Column("CLA1286_KafkaFlag"));
            Property(x => x.CLA3014_LoadDataToSnowflake, m => m.Column("CLA3014_LoadDataToSnowflake"));
            this.Bag((x) => x.Revisions, (m) =>
            {
                m.Inverse(true);
                m.Table("SchemaRevision");
                m.Cascade(Cascade.All);
                m.Key((k) =>
                {
                    k.Column("ParentSchema_Id");
                    k.ForeignKey("FK_SchemaRevision_Schema");
                });
            }, map => map.OneToMany(a => a.Class(typeof(SchemaRevision))));
        }

        public class FileSchemaMapping : SubclassMapping<FileSchema>
        {
            public FileSchemaMapping()
            {
                DiscriminatorValue("FileSchema");

                this.ManyToOne(x => x.Extension, m =>
                {
                    m.Column("FileExtension_Id");
                    m.ForeignKey("FK_Schema_FileExtension");
                    m.Class(typeof(FileExtension));
                });
                Property(x => x.Delimiter, m => m.Column("Delimiter"));
                Property(x => x.HasHeader, m => m.Column("HasHeader"));
                Property(x => x.CreateCurrentView, m => m.Column("CreateCurrentView"));
                Property(x => x.SasLibrary, m => m.Column("SASLibrary"));
                Property(x => x.HiveTable, m => m.Column("HiveTable"));
                Property(x => x.HiveDatabase, m => m.Column("HiveDatabase"));
                Property(x => x.HiveLocation, m => m.Column("HiveLocation"));
                Property(x => x.StorageCode, m => m.Column("StorageCode"));
                Property(x => x.HiveTableStatus, m => m.Column("HiveStatus"));
                Property(x => x.SchemaRootPath, m => m.Column("SchemaRootPath"));
                Property(x => x.ParquetStorageBucket);
                Property(x => x.ParquetStoragePrefix);
                Bag((x) => x.ConsumptionDetails, (m) =>
                {
                    m.Inverse(true);
                    m.Table("SchemaConsumption");
                    m.Cascade(Cascade.All);
                    m.Key((k) =>
                    {
                        k.Column("Schema_Id");
                        k.ForeignKey("FK_SchemaConsumption_Schema");
                    });
                }, map => map.OneToMany(a => a.Class(typeof(SchemaConsumption))));
            }
        }
    }
}

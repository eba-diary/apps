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
            this.Bag((x) => x.Revisions, (m) =>
            {
                m.Inverse(true);
                m.Table("SchemaRevision");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
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
                Property(x => x.IsInSAS, m => m.Column("IsInSAS"));
                Property(x => x.CreateCurrentView, m => m.Column("CreateCurrentView"));
                Property(x => x.SasLibrary, m => m.Column("SASLibrary"));
            }
        }
    }
}

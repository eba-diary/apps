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
            //Bag(x => x.Revisions, (m) =>
            //{
            //    m.Inverse(true);
            //    m.Table("SchemaRevision");
            //    m.Cascade(Cascade.All);
            //    m.Key((k) =>
            //    {
            //        k.Column("Revision_Id");
            //    });
            //}, map => map.OneToMany(a => a.Class(typeof(SchemaRevision))));
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
            }
        }
    }
}

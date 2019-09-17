using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class FileSchemaMapping : ClassMapping<FileSchema>
    {
        public FileSchemaMapping()
        {
            Table("FileSchema");

            Id(x => x.SchemaId, m =>
            {
                m.Column("Schema_Id");
                m.Generator(Generators.GuidComb);
            });

            Property((x) => x.Name, (m) => m.Column("Schema_NME"));
            Property((x) => x.RevisionId, m => m.Column("Revision_Id"));
            Property((x) => x.RevisionName, m => m.Column("Revision_NME"));
            ManyToOne(x => x.Extension, m =>
            {
                m.Column("FileExtension_Id");
                m.ForeignKey("FK_FileSchema_FileExtension");
                m.Class(typeof(FileExtension));
            });
            Bag(x => x.Fields, (m) =>
            {
                m.Inverse(true);
                m.Table("SchemaField");
                m.Cascade(Cascade.All);
                m.Key((k) =>
                {
                    k.Column("Field_Id");
                });
            }, map => map.OneToMany(a => a.Class(typeof(BaseField))));
        }
    }
}

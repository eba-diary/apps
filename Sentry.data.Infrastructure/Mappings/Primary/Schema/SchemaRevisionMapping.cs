using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SchemaRevisionMapping : ClassMapping<SchemaRevision>
    {
        public SchemaRevisionMapping()
        {
            Table("SchemaRevision");

            Id(x => x.SchemaRevision_Id, m =>
            {
                m.Column("SchemaRevision_Id");
                m.Generator(Generators.Identity);
            });

            this.ManyToOne(x => x.ParentSchema, m =>
            {
                m.Column("ParentSchema_Id");
                m.ForeignKey("FK_SchemaRevision_Schema");
                //m.Cascade(Cascade.All);
                m.Class(typeof(Schema));
            });

            Property((x) => x.Revision_NBR, (m) => m.Column("Revision_NBR"));
            Property((x) => x.SchemaRevision_Name, (m) => m.Column("Revision_Name"));
            Property((x) => x.CreatedBy, (m) => m.Column("CreatedBy"));
            Property((x) => x.CreatedDTM, (m) => m.Column("CreatedDTM"));
            Property((x) => x.LastUpdatedDTM, (m) => m.Column("LastUpdatedDTM"));

            this.Bag((x) => x.Fields, (m) =>
            {
                m.Inverse(true);
                m.Table("SchemaField");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Key((k) =>
                {
                    k.Column("ParentSchemaRevision");
                    k.ForeignKey("FK_SchemaField_SchemaRevision");
                });
            }, map => map.OneToMany(a => a.Class(typeof(BaseField))));
        }
    }
}

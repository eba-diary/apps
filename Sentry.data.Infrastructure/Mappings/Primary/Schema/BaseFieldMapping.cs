using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class BaseFieldMapping : ClassMapping<BaseField>
    {
        public BaseFieldMapping()
        {
            Table("SchemaField");

            Id(x => x.FieldId, m =>
            {
                m.Column("Field_Id");
                m.Generator(Generators.GuidComb);
            });

            Property((x) => x.Name, (m) => m.Column("Field_NME"));
            Property((x) => x.IsArray, (m) => m.Column("IsArray"));
            Property((x) => x.OrdinalPosition, (m) => m.Column("OrdinalPosition"));
            Property((x) => x.StartPosition, (m) => m.Column("StartPosition"));
            Property((x) => x.EndPosition, (m) => m.Column("EndPosition"));
            Property((x) => x.CreateDTM, (m) => m.Column("CreateDTM"));
            Property((x) => x.LastUpdateDTM, (m) => m.Column("LastUpdateDTM"));
            Property((x) => x.NullableIndicator, (m) => m.Column("NullableIndicator"));
            Property((x) => x.Type, (m) => m.Column("Type"));

            ManyToOne((x) => x.ParentField, (m) =>
            {
                m.Access(Accessor.Field);
                m.ForeignKey("FK_SchemaField_SchemaField");
            });

            Bag((x) => x.ChildFields, (c) =>
            {
                c.Inverse(true);
                c.Cascade(Cascade.All);
                c.Access(Accessor.Field);
            },
            (m) => m.OneToMany());
        }
    }
}

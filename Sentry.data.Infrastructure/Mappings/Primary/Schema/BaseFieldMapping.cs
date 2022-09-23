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
                m.Generator(Generators.Identity);
            });

            Property((x) => x.Name, (m) => m.Column("Field_NME"));
            Property((x) => x.IsArray, (m) => m.Column("IsArray"));
            Property((x) => x.OrdinalPosition, (m) => m.Column("OrdinalPosition"));
            Property((x) => x.StartPosition, (m) => m.Column("StartPosition"));
            Property((x) => x.EndPosition, (m) => m.Column("EndPosition"));
            Property((x) => x.CreateDTM, (m) => m.Column("CreateDTM"));
            Property((x) => x.LastUpdateDTM, (m) => m.Column("LastUpdateDTM"));
            Property((x) => x.NullableIndicator, (m) => m.Column("NullableIndicator"));
            Property((x) => x.FieldGuid, (m) => m.Column("FieldGuid"));
            Property((x) => x.Description, (m) => m.Column("Description"));
            Property(x => x.FieldLength, m => m.Column("FieldLength"));
            Property((x) => x.DotNamePath, (m) => m.Column("DotNamePath"));
            Property(x => x.StructurePosition, m => m.Column("StructurePosition"));

            Discriminator(x => x.Column("Type"));

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

            this.ManyToOne(x => x.ParentSchemaRevision, m =>
            {
                m.Column("ParentSchemaRevision");
                m.ForeignKey("FK_SchemaField_SchemaRevision");
                m.Class(typeof(SchemaRevision));
            });
        }

        public class StructFieldMapping : SubclassMapping<StructField>
        {
            public StructFieldMapping()
            {
                DiscriminatorValue("STRUCT");
            }
        }

        public class IntegerFieldMapping : SubclassMapping<IntegerField>
        {
            public IntegerFieldMapping()
            {
                DiscriminatorValue("INTEGER");
            }
        }

        public class BigintFieldMapping : SubclassMapping<BigIntField>
        {
            public BigintFieldMapping()
            {
                DiscriminatorValue("BIGINT");
            }
        }

        public class DecimalFieldMapping : SubclassMapping<DecimalField>
        {
            public DecimalFieldMapping()
            {
                DiscriminatorValue("DECIMAL");

                Property(x => x.Precision, m => m.Column("Precision"));
                Property(x => x.Scale, m => m.Column("Scale"));
            }
        }

        public class DateFieldMapping : SubclassMapping<DateField>
        {
            public DateFieldMapping()
            {
                DiscriminatorValue("DATE");

                Property(x => x.SourceFormat, m => m.Column("SourceFormat"));
            }
        }

        public class TimestampFieldMapping : SubclassMapping<TimestampField>
        {
            public TimestampFieldMapping()
            {
                DiscriminatorValue("TIMESTAMP");

                Property(x => x.SourceFormat, m => m.Column("SourceFormat"));
            }
        }

        public class VarcharFieldMapping : SubclassMapping<VarcharField>
        {
            public VarcharFieldMapping()
            {
                DiscriminatorValue("VARCHAR");
            }
        }
    }
}

using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;


namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class LineageMapping : ClassMapping<Lineage>
    {
        public LineageMapping()
        {          
            this.Table("MetadataRepository.dbo.vw_DataLineage");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id(x => x.ID, m =>
            {
                m.Generator(Generators.GuidComb);
            });

            this.Property((x) => x.DataAsset_ID, (m) => m.Column("DataAsset_ID"));
            this.Property((x) => x.DataElement_NME, (m) => m.Column("DataElement_NME"));
            this.Property((x) => x.DataObject_NME, (m) => m.Column("DataObject_NME"));
            this.Property((x) => x.DataObjectCode_DSC, (m) => m.Column("DataObjectCode_DSC"));
            this.Property((x) => x.DataObjectDetailType_VAL, (m) => m.Column("DataObjectDetailType_VAL"));
            this.Property((x) => x.DataObjectField_NME, (m) => m.Column("DataObjectField_NME"));
            this.Property((x) => x.SourceElement_NME, (m) => m.Column("SourceElement_NME"));
            this.Property((x) => x.SourceObject_NME, (m) => m.Column("SourceObject_NME"));
            this.Property((x) => x.SourceObjectField_NME, (m) => m.Column("SourceObjectField_NME"));
            this.Property((x) => x.Source_TXT, (m) => m.Column("Source_TXT"));
            this.Property((x) => x.Transformation_TXT, (m) => m.Column("Transformation_TXT"));
            this.Property((x) => x.Display_IND, (m) => m.Column("Display_IND"));

        }

    }
}

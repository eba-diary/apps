using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode.Impl;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataLineageMapping : ClassMapping<Lineage>
    {
        public DataLineageMapping()
        {
            this.Table("vw_DataLineage");

            this.Id(x => x.DataLineage_ID);

            this.Property(x => x.DataAsset_ID);
            this.Property(x => x.Line_CDE);

            this.Property(x => x.Model_NME);

            this.Property(x => x.DataElement_NME);
            this.Property(x => x.DataElement_TYP);

            this.Property(x => x.DataObject_NME);
            this.Property(x => x.DataObjectCode_DSC);
            this.Property(x => x.DataObjectField_NME);

            this.Property(x => x.SourceElement_NME);
            this.Property(x => x.SourceObject_NME);
            this.Property(x => x.SourceField_NME);

            this.Property(x => x.Source_TXT);
            this.Property(x => x.Transformation_TXT);
            this.Property(x => x.BusTerm_DSC);

            this.Property(x => x.Display_IND);

        }
    }
}

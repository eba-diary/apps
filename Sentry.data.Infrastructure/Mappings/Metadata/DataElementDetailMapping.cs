using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataElementDetailMapping : ClassMapping<DataElementDetail>
    {
        public DataElementDetailMapping()
        {
            this.Table("MetadataRepository.dbo.DataElementDetail");

            this.Id(x => x.DataElementDetail_ID);


            this.ManyToOne(x => x.DataElement, m =>
            {
                m.Column("DataElement_ID");
                m.Class(typeof(DataElement));
            });

            this.Property(x => x.DataElementDetailCreate_DTM);
            this.Property(x => x.DataElementDetailChange_DTM);
            this.Property(x => x.DataElementDetailType_CDE);
            this.Property(x => x.DataElementDetailType_VAL);
            this.Property(x => x.LastUpdt_DTM);
            this.Property(x => x.BusElementKey);

        }
    }
}

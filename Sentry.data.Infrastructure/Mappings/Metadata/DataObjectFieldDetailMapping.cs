using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Metadata;
using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataObjectFieldDetailMapping : ClassMapping<DataObjectFieldDetail>
    {
        public DataObjectFieldDetailMapping()
        {
            this.Lazy(false);

            this.Table(Sentry.Configuration.Config.GetHostSetting("MetadataRepository") + ".dbo.DataObjectFieldDetail");

            this.Id(x => x.DataObjectFieldDetail_ID, m => m.Generator(Generators.Identity));

            //this.ManyToOne(x => x.DataObjectField, m =>
            //{
            //    m.Column("DataObjectField_ID");
            //    m.Class(typeof(DataObjectField));
            //});

            this.Property(x => x.DataObjectField_ID);
            this.Property(x => x.DataTag_ID);
            this.Property(x => x.DataObjectFieldDetailCreate_DTM);
            this.Property(x => x.DataObjectFieldDetailChange_DTM);
            this.Property(x => x.DataObjectFieldDetailType_CDE);
            this.Property(x => x.DataObjectFieldDetailType_VAL);
            this.Property(x => x.LastUpdt_DTM);
            //this.Property(x => x.BusFieldKey);
        }
    }
}

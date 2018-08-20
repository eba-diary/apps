using NHibernate.Mapping.ByCode;
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
    public class DataObjectDetailMapping : ClassMapping<DataObjectDetail>
    {
        public DataObjectDetailMapping()
        {
            this.Lazy(false);

            this.Table(Sentry.Configuration.Config.GetHostSetting("MetadataRepository") + ".dbo.DataObjectDetail");

            this.Id(x => x.DataObjectDetail_ID, m => m.Generator(Generators.Identity));

            //this.ManyToOne(x => x.DataObject, m =>
            //{
            //    m.Column("DataObject_ID");
            //    m.Class(typeof(DataObject));
            //});

            //this.Property(x => x.DataObject_ID);
            this.Property(x => x.DataObjectDetailCreate_DTM);
            this.Property(x => x.DataObjectDetailChange_DTM);
            this.Property(x => x.DataObjectDetailType_CDE);
            this.Property(x => x.DataObjectDetailType_VAL);
            this.Property(x => x.LastUpdt_DTM);
            this.Property(x => x.BusObjectKey);
        }
    }
}

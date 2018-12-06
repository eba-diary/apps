
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
    public class DataObjectFieldMapping : ClassMapping<DataObjectField>
    {
        public DataObjectFieldMapping()
        {
            this.Lazy(false);

            this.Table("DataObjectField");

            this.Id(x => x.DataObjectField_ID, m => m.Generator(Generators.Identity));

            //this.ManyToOne(x => x.DataObject, m =>
            //{
            //    m.Column("DataObject_ID");
            //    m.Class(typeof(DataObject));
            //});

            this.Property(x => x.DataObject_ID);
            this.Property(x => x.DataTag_ID);
            this.Property(x => x.DataObjectField_NME);
            this.Property(x => x.DataObjectField_DSC);
            this.Property(x => x.DataObjectFieldCreate_DTM);
            this.Property(x => x.DataObjectFieldChange_DTM);
            this.Property(x => x.LastUpdt_DTM);
            //this.Property(x => x.BusObjectKey);
            //this.Property(x => x.BusFieldKey);

            this.Bag(x => x.DataObjectFieldDetails, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(false);
                m.Table("DataObjectFieldDetail");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));

                m.Key((k) =>
                {
                    k.Column("DataObjectField_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataObjectFieldDetail))));
        }
    }
}

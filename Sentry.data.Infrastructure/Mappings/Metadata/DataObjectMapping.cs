using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataObjectMapping : ClassMapping<DataObject>
    {
        public DataObjectMapping()
        {
            this.Lazy(false);

            this.Table("DataObject");

            this.Id(x => x.DataObject_ID, m => m.Generator(Generators.Identity));


            //this.ManyToOne(x => x.DataElement, m =>
            //{
            //    m.Column("DataElement_ID");
            //    m.Class(typeof(DataElement));
            //});


            this.Property(x => x.DataTag_ID);
            this.Property(x => x.Reviewer_ID);
            this.Property(x => x.DataObject_NME);
            this.Property(x => x.DataObject_DSC);
            this.Property(x => x.DataObjectParent_ID);
            this.Property(x => x.DataObject_CDE);
            this.Property(x => x.DataObjectCode_DSC);
            this.Property(x => x.DataObjectCreate_DTM);
            this.Property(x => x.DataObjectChange_DTM);
            this.Property(x => x.LastUpdt_DTM);
            this.Property(x => x.BusElementKey);
            this.Property(x => x.BusObjectKey);

            this.Bag(x => x.DataObjectDetails, (m) =>
            {

                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(false);
                m.Table("DataObjectDetail");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));

                m.Key((k) =>
                {
                    k.Column("DataObject_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataObjectDetail))));

            this.Bag(x => x.DataObjectFields, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(false);
                m.Table("DataObjectField");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));

                m.Key((k) =>
                {
                    k.Column("DataObject_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataObjectField))));
        }
    }
}

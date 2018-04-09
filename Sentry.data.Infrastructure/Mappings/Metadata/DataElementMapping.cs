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
    public class DataElementMapping : ClassMapping<DataElement>
    {
        public DataElementMapping()
        {
            this.Table("MetadataRepository.dbo.DataElement");

            this.Id(x => x.DataElement_ID);

            this.ManyToOne(x => x.MetadataAsset, m =>
            {
                m.Column("DataAsset_ID");
                m.Class(typeof(MetadataAsset));
            });

            this.Property(x => x.DataTag_ID);
            this.Property(x => x.DataElement_NME);
            this.Property(x => x.DataElement_DSC);
            this.Property(x => x.DataElement_CDE);
            this.Property(x => x.DataElementCode_DSC);
            this.Property(x => x.DataElementCreate_DTM);
            this.Property(x => x.DataElementChange_DTM);
            this.Property(x => x.LastUpdt_DTM);
            this.Property(x => x.BusElementKey);

            this.Bag(x => x.DataElementDetails, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("MetadataRepository.dbo.DataElementDetail");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));

                m.Key((k) =>
                {
                    k.Column("DataElement_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataElementDetail))));

            this.Bag(x => x.DataObjects, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("MetadataRepository.dbo.DataObject");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));

                m.Key((k) =>
                {
                    k.Column("DataObject_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataObject))));


        }
    }
}

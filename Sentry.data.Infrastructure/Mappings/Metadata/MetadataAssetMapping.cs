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
    public class MetadataAssetMapping : ClassMapping<MetadataAsset>
    {
        public MetadataAssetMapping()
        {
            this.Table(Sentry.Configuration.Config.GetHostSetting("MetadataRepository") + ".dbo.DataAsset");

            this.Id(x => x.DataAsset_ID);

            this.Property(x => x.DataAsset_NME);
            this.Property(x => x.DataAsset_DSC);
            this.Property(x => x.DataAssetOwner_NME);
            this.Property(x => x.LastUpdt_DTM);

            this.Bag(x => x.DataElements, (m) =>
            {
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table(Sentry.Configuration.Config.GetHostSetting("MetadataRepository") + ".dbo.DataElement");
                m.Cascade(Cascade.All);
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));

                m.Key((k) =>
                {
                    k.Column("DataAsset_ID");
                });
            }, map => map.OneToMany(a => a.Class(typeof(DataElement))));
        }
    }
}

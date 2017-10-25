using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class AssetSourceMapping : ClassMapping<AssetSource>
    {
        public AssetSourceMapping()
        {
            this.Table("AssetSource");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.SourceId, m =>
            {
                m.Column("Source_ID");
            });

            this.Property(x => x.DisplayName, m => m.Column("SourceDisplay_NME"));
            this.Property(x => x.Description, m => m.Column("Source_DSC"));
            this.Property(x => x.MetadataRepositorySrcSysName, m => m.Column("MetadataRepositorySrcSys_VAL"));
            this.Property(x => x.DataAssetId, m => m.Column("DataAsset_ID"));
            this.Property(x => x.IsVisiable, m => m.Column("IsVisible_IND"));
        }
    }
}

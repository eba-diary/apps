using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class BusinessAreaTileRowMapping : ClassMapping<BusinessAreaTileRow>
    {
        public BusinessAreaTileRowMapping()
        {
            this.Table("BusinessAreaTileRow");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("BusinessAreaTileRow_Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.ColumnSpan, m => m.Column("NbrOfColumns_CNT"));
            this.Property(x => x.BusinessAreaType, m => m.Column("BusinessArea_ID"));
            this.Property(x => x.Sequence, m => m.Column("Order_SEQ"));

            this.Bag<BusinessAreaTile>(x => x.Tiles, (b) =>
            {
                b.Table("BusinessAreaTileRow_BusinessAreaTile");
                b.Inverse(false);
                b.Key((k) =>
                {
                    k.Column("BusinessAreaTileRow_ID");
                    k.ForeignKey("FK_BusinessAreaTileRow_BusinessAreaTile_BusinessAreaTileRow");
                });
            },
            map => map.ManyToMany(n =>
            {
                n.Column("BusinessAreaTile_ID");
                n.ForeignKey("FK_BusinessAreaTileRow_BusinessAreaTile_BusinessAreaTile");
            }));
        }
    }
}

using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class BusinessAreaTileMapping : ClassMapping<BusinessAreaTile>
    {
        public BusinessAreaTileMapping()
        {
            this.Table("BusinessAreaTile");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("BusinessAreaTile_Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Title, m => m.Column("Title_DSC"));
            this.Property(x => x.TileColor, m => m.Column("TileColor_DSC"));
            this.Property(x => x.ImageName, m => m.Column("Image_NME"));
            this.Property(x => x.LinkText, m => m.Column("Hyperlink_DSC"));
            this.Property(x => x.Hyperlink, m => m.Column("Hyperlink_URL"));
            this.Property(x => x.Sequence, m => m.Column("Order_SEQ"));
        }
    }
}
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class FavoriteMapping : ClassMapping<Favorite>
    {
        public FavoriteMapping()
        {
            this.Table("Favorites");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.FavoriteId, (m) =>
            {
                m.Column("FavoriteId");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.DatasetId, (m) => m.Column("DatasetId"));
            this.Property((x) => x.UserId, (m) => m.Column("UserId"));
            this.Property((x) => x.Created, (m) => m.Column("Created"));
        }
    }
}

using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class UserFavoriteMapping : ClassMapping<UserFavorite>
    {
        public UserFavoriteMapping()
        {
            Table("UserFavorite");
            Cache((c) => c.Usage(CacheUsage.ReadWrite));
            Id(x => x.UserFavoriteId, x =>
            {
                x.Column("UserFavoriteId");
                x.Generator(Generators.Identity);
            });
            Property(x => x.AssociateId, x =>
            {
                x.Column("AssociateId");
                x.NotNullable(true);
            });
            Property(x => x.FavoriteType, x =>
            {
                x.Column("FavoriteType");
                x.NotNullable(true);
            });
            Property(x => x.FavoriteEntityId, x =>
            {
                x.Column("FavoriteEntityId");
                x.NotNullable(true);
            });
            Property(x => x.Sequence, x =>
            {
                x.Column("Sequence");
                x.NotNullable(true);
            });
            Property(x => x.CreateDate, x => 
            {
                x.Column("CreateDate");
                x.NotNullable(true);
            });
        }
    }
}

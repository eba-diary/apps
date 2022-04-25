using System;

namespace Sentry.data.Core
{
    public class UserFavorite
    {
        public virtual int UserFavoriteId { get; set; }
        public virtual string AssociateId { get; set; }
        public virtual string FavoriteType { get; set; }
        public virtual int FavoriteEntityId { get; set; }
        public virtual int Sequence { get; set; }
        public virtual DateTime CreateDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Favorite
    {
        public virtual int FavoriteId { get; set; }
        public virtual int DatasetId { get; set; }
        public virtual string UserId { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual int Sequence { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class FavoriteDto
    {
        public virtual string UserId { get; set; }
        public virtual string UserEmail { get; set; }
        public virtual string UserDisplayName { get; set; }
    }
}

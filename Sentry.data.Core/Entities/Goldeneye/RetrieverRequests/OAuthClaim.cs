using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class OAuthClaim
    {
        public virtual int Id { get; set; }
        public virtual DataSource DataSourceId { get; set; }
        public virtual int TokenId { get; set; }
        public virtual OAuthClaims Type { get; set; } 
        public virtual string Value { get; set; }
    }
}

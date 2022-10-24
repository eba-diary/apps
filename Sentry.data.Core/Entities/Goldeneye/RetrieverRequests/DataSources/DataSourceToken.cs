using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataSourceToken
    {
        public virtual int Id { get; set; }
        public virtual DataSource ParentDataSource { get; set; }
        
        [DisplayName("Value")]
        public virtual string CurrentToken { get; set; }
        
        [DisplayName("Refresh Value")]
        public virtual string RefreshToken { get; set; }
        
        [DisplayName("Expiration Time")]
        public virtual DateTime ?CurrentTokenExp { get; set; }

        [DisplayName("Name")]
        public virtual string TokenName { get; set; }

        [DisplayName("Url")]
        public virtual string TokenUrl { get; set; }

        [DisplayName("Expiration (in Seconds)")]
        public virtual int TokenExp { get; set; }

        [DisplayName("Scope")]
        public virtual string Scope { get; set; }

        public virtual IList<OAuthClaim> Claims { get; set; }
    }
}

using Newtonsoft.Json;
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

        [JsonIgnore]
        public virtual DataSource ParentDataSource { get; set; }
        
        public virtual string CurrentToken { get; set; }
        
        public virtual string RefreshToken { get; set; }
        
        public virtual DateTime ?CurrentTokenExp { get; set; }

        public virtual string TokenName { get; set; }

        public virtual string TokenUrl { get; set; }

        public virtual int TokenExp { get; set; }

        public virtual string Scope { get; set; }

        public virtual string ForeignId { get; set; }

        public virtual IList<OAuthClaim> Claims { get; set; }

        public virtual bool Enabled { get; set; }

        public virtual bool AcceptableErrorNeedsReview { get; set; }
    }
}

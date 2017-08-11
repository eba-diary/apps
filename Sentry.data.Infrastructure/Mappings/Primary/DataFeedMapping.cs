using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataFeedMapping : ClassMapping<DataFeed>
    {
        public DataFeedMapping()
        {
            this.Table("DataFeed");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("Feed_ID");
                m.Generator(Generators.Identity);
            });
            
            this.Property(x => x.Url, m => m.Column("Feed_URL"));
            this.Property(x => x.Type, m => m.Column("FeedType_CDE"));
            this.Property(x => x.Name, m => m.Column("Feed_NME"));
        }
    }
}

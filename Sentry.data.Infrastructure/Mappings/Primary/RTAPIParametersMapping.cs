using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class RTAPIParametersMapping : ClassMapping<RTAPIParameters>
    {
        public RTAPIParametersMapping()
        {
            this.Table("RT_API_Parameters");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("APIParameter_Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.SourceTypeId, m => m.Column("SourceType_ID"));
            this.Property(x => x.ApiEndpointId, m => m.Column("APIEndpoint_ID"));
            this.Property(x => x.Name, m => m.Column("Name"));

            //this.ManyToOne
        }
    }
}

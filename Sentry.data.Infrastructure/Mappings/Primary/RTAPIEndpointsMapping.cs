using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class RTAPIEndpointsMapping : ClassMapping<RTAPIEndpoints>
    {
        public RTAPIEndpointsMapping()
        {
            this.Table("RT_API_Endpoints");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("Endpoint_Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.SourceTypeId, m => m.Column("SourceType_ID"));
            this.Property(x => x.Name, m => m.Column("Name"));
            this.Property(x => x.Value, m => m.Column("Value"));

            this.Bag(x => x.Parameters, m =>
            {
                m.Cache(c => c.Usage(CacheUsage.ReadOnly));
                m.Inverse(true);
                m.Table("RT_API_Parameters");
                m.Cascade(Cascade.All);
                m.Key(k =>
                {
                    k.Column("APIEndpoint_Id");
                    k.ForeignKey("FK_RT_API_Parameters_RT_API_Endpoints");
                });
            }, map => map.OneToMany(a => a.Class(typeof(RTAPIParameters))));

        }
    }
}

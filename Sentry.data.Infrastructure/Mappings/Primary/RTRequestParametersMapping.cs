using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class RTRequestParametersMapping : ClassMapping<RTRequestParameters>
    {
        public RTRequestParametersMapping()
        {
            this.Table("RT_Request_Parameters");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("RequestParameter_Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.RequestId, m => m.Column("Request_ID"));
            this.Property(x => x.ApiParameterId, m => m.Column("APIParameter_ID"));
            this.Property(x => x.Value, m => m.Column("Value"));

            this.ManyToOne(x => x.ApiParameter, m =>
            {
                m.Column("APIParameter_ID");
                m.ForeignKey("FK_RT_Request_Parameters_RT_APIParameters");
                m.Class(typeof(RTAPIParameters));
                m.Update(false);
                m.Insert(false);
            });
        }
    }
}

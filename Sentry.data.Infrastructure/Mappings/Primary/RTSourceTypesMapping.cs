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
    public class RTSourceTypesMapping : ClassMapping<RTSourceTypes>
    {
        public RTSourceTypesMapping()
        {
            this.Table("RT_Source_Types");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("SourceType_Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Name, m => m.Column("Name"));
            this.Property(x => x.BaseUrl, m => m.Column("BaseURL"));
            this.Property(x => x.Description, m => m.Column("Description"));
            this.Property(x => x.Type, m => m.Column("Type_NME"));

            this.Bag(x => x.Endpoints, m =>
            {
                m.Cache(c => c.Usage(CacheUsage.ReadOnly));
                m.Inverse(true);
                m.Table("RT_API_Endpoints");
                m.Cascade(Cascade.All);
                m.Key(k =>
                {
                    k.Column("SourceType_Id");
                    k.ForeignKey("FK_RT_API_Endpoints_RT_Source_Types");
                });
            }, map => map.OneToMany(a => a.Class(typeof(RTAPIEndpoints))));
        }
    }
}

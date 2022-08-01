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
    public class RTRequestMapping : ClassMapping<RTRequest>
    {
        public RTRequestMapping()
        {
            this.Table("RT_Request");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("Request_Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.SourceTypeId, m => m.Column("SourceType_ID"));
            this.Property(x => x.EndpointId, m => m.Column("APIEndpoint_ID"));
            this.Property(x => x.IsEnabled, m => m.Column("Enable_IND"));
            this.Property(x => x.SystemFolder, m => m.Column("SystemFolder_NME"));
            this.Property(x => x.RequestName, m => m.Column("Request_NME"));
            this.Property(x => x.Options, m => m.Column("Options_DSC"));

            this.ManyToOne(x => x.SourceType, m =>
            {
                m.Column("SourceType_ID");
                m.ForeignKey("FK_RT_Request_RT_Source_Types");
                m.Class(typeof(RTSourceTypes));
                m.Insert(false);
                m.Update(false);
            });

            this.ManyToOne(x => x.Endpoint, m =>
            {
                m.Column("APIEndpoint_ID");
                m.ForeignKey("FK_RT_Request_RT_API_Endpoints");
                m.Class(typeof(RTAPIEndpoints));
                m.Insert(false);
                m.Update(false);
            });

            //this.Bag(x => x.Parameters, m =>
            //{
            //    m.Cache(c => c.Usage(CacheUsage.ReadOnly));
            //    m.Inverse(true);
            //    m.Table("RT_Request_Parameters");
            //    m.Cascade(Cascade.All);
            //    m.Key(k =>
            //    {
            //        k.Column("Request_ID");
            //        k.ForeignKey("FK_RT_Request_Parameters_RT_Request");
            //    });
            //}, map => map.OneToMany(a => a.Class(typeof(RTRequestParameters))));

            this.Bag(x => x.Parameters, m =>
            {
                m.Cache(c => c.Usage(CacheUsage.ReadOnly));
                m.Inverse(true);
                m.Table("RT_Request_Parameters");
                m.Cascade(Cascade.All);
                m.Key(k =>
                {
                    k.Column("Request_ID");
                    k.ForeignKey("FK_RT_Request_Parameters_RT_Request");
                });
            }, map => map.OneToMany(a => a.Class(typeof(RTRequestParameters))));
        }
    }
}

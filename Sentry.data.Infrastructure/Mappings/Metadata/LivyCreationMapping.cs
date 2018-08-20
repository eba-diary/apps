using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class LivyCreationMapping : ClassMapping<LivyCreation>
    {
        public LivyCreationMapping()
        {
            this.Table("Livy_Sessions");

            this.Cache((c) => c.Usage(CacheUsage.ReadWrite));

            this.Id((x) => x.ID, (m) =>
            {
                m.Column("ID");
                m.Generator(Generators.Identity);
            });

            this.Property((x) => x.LivySession_ID, (m) => m.Column("LivySession_ID"));

            this.Property((x) => x.Session_NME, (m) => m.Column("Session_NME"));
            this.Property((x) => x.Associate_ID, (m) => m.Column("Associate_ID"));
            this.Property((x) => x.Active_IND, (m) => m.Column("Active_IND"));
            this.Property((x) => x.Start_DTM, (m) => m.Column("Start_DTM"));
            this.Property((x) => x.End_DTM, (m) => m.Column("End_DTM"));
            this.Property((x) => x.ForDSC_IND, (m) => m.Column("ForDSC_IND"));

            this.Property((x) => x.Kind, (m) => m.Column("Kind"));

            this.Property((x) => x.Jars, (m) => m.Column("Jars"));
            this.Property((x) => x.PyFiles, (m) => m.Column("PyFiles"));

            this.Property((x) => x.DriverMemory, (m) => m.Column("DriverMemory"));
            this.Property((x) => x.DriverCores, (m) => m.Column("DriverCores"));

            this.Property((x) => x.ExecutorMemory, (m) => m.Column("ExecutorMemory"));
            this.Property((x) => x.ExecutorCores, (m) => m.Column("ExecutorCores"));
            this.Property((x) => x.NumExecutors, (m) => m.Column("NumExecutors"));

            this.Property((x) => x.Queue, (m) => m.Column("Queue"));
            this.Property((x) => x.HeartbeatTimeoutInSecond, (m) => m.Column("HeartbeatTimeoutInSecond"));
        }
    }
}

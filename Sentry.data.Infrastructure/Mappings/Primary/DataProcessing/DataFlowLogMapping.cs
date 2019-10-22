using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;
using NHibernate;
using NHibernate.Type;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataFlowLogMapping : ClassMapping<DataFlow_Log>
    {
        public DataFlowLogMapping()
        {
            this.Table("DataFlowLog");

            this.Id(x => x.Log_Id, m =>
            {
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Log_Entry, c =>
            {
                c.Column("Log_Entry");
                c.Type(NHibernateUtil.StringClob);
            });

            this.Property(x => x.FlowExecutionGuid, c => c.Column("FlowExecutionGuid"));
            this.Property(x => x.Level, c =>
            {
                c.Column("Level");
                c.Type<EnumType<Log_Level>>();
            });
            this.Property(x => x.Machine_Name, c => c.Column("Machine_Name"));
            this.Property(x => x.CreatedDTM, c => c.Column("CreatedDTM"));
            this.ManyToOne(x => x.Step, m =>
            {
                m.Column("DataFlowStep_Id");
                m.Class(typeof(DataFlowStep));
            });
            this.ManyToOne(x => x.DataFlow, m =>
            {
                m.Column("DataFlow_Id");
                m.Class(typeof(DataFlow));
            });
        }
    }
}

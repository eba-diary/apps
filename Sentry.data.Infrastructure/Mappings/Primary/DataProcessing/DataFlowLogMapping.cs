using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataFlowLogMapping : ClassMapping<EventMetric>
    {
        public DataFlowLogMapping()
        {
            this.Table("EventMetrics");

            this.Id(x => x.EventMetricsId, m =>
            {
                m.Generator(Generators.Identity);
            });

            //this.Property(x => x.Log_Entry, c =>
            //{
            //    c.Column("Log_Entry");
            //    c.Type(NHibernateUtil.StringClob);
            //});

            this.Property(x => x.FlowExecutionGuid, c => c.Column("FlowExecutionGuid"));
            this.Property(x => x.RunInstanceGuid, c => c.Column("RunInstanceGuid"));
            this.ManyToOne(x => x.Step, m =>
            {
                m.Column("DataFlowStepId");
                m.Class(typeof(DataFlowStep));
            });
            this.Property(x => x.ServiceRunGuid, c => c.Column("ServiceRunGuid"));
            this.Property(x => x.ProcessRunGuid, c => c.Column("ProcessRunGuid"));
            this.Property(x => x.Partition, c => c.Column("Partition"));
            this.Property(x => x.Offset, c => c.Column("Offset"));
            this.Property(x => x.MessageKey, c => c.Column("MessageKey"));
            this.Property(x => x.MessageValue, m => { m.Column("MessageValue"); m.Type(NHibernateUtil.StringClob); });
            this.Property(x => x.ApplicationName, c => c.Column("ApplicationName"));
            this.Property(x => x.MachineName, c => c.Column("MachineName"));
            this.Property(x => x.StatusCode, c => c.Column("StatusCode"));
            this.Property(x => x.MetricsData, m => { m.Column("MetricsData"); m.Type(NHibernateUtil.StringClob); });
            this.Property(x => x.CreatedDTM, c => c.Column("CreatedDTM"));
        }
    }
}

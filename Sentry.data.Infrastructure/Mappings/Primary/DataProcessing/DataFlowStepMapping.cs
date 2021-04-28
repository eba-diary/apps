﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataFlowStepMapping : ClassMapping<DataFlowStep>
    {
        public DataFlowStepMapping()
        {
            this.Table("DataFlowStep");

            this.Id((x) => x.Id, (m) =>
            {
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.ExeuctionOrder, m => m.Column("ExecutionOrder"));
            this.Property(x => x.DataAction_Type_Id,
               attr => attr.Type<EnumType<DataActionType>>());
            this.Property(x => x.TriggerKey, m => m.Column("TriggerKey"));
            this.Property(x => x.TargetPrefix, m => m.Column("TargetPrefix"));
            this.Property(x => x.SourceDependencyPrefix, m => m.Column("SourceDependencyPrefix"));
            this.Property(x => x.SourceDependencyBucket, m => m.Column("SourceDependencyBucket"));

            this.ManyToOne(x => x.DataFlow, m =>
            {
                m.Column("DataFlow_ID");
                m.Class(typeof(DataFlow));
            });

            this.ManyToOne(x => x.Action, m =>
            {
                m.Column("Action_ID");
                m.ForeignKey("FK_DataFlowStep_DataAction");
                m.Class(typeof(BaseAction));
                m.Update(false);
            });

            this.Bag(x => x.Executions, (m) =>
            {
                m.Inverse(true);
                m.Table("EventMetrics");
                m.Cascade(Cascade.All);
                m.Key((k) =>
                {
                    k.Column("DataFlowStepId");
                });
            }, map => map.OneToMany(a => a.Class(typeof(EventMetric))));

            this.Bag(x => x.SchemaMappings, (m) =>
            {
                m.Inverse(true);
                m.Table("DataStepToSchema");
                m.Cascade(Cascade.All);
                m.Key((k) =>
                {
                    k.Column("DataFlowStepId");
                });
            }, map => map.OneToMany(a => a.Class(typeof(SchemaMap))));
        }
    }
}
using NHibernate.Mapping.ByCode;
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

            this.ManyToOne(x => x.DataFlow, m =>
            {
                m.Column("DataFlow_ID");
                m.Class(typeof(DataFlow));
            });

            this.ManyToOne(x => x.Action, m =>
            {
                m.Column("DataAction_ID");
                m.Class(typeof(BaseAction));
            });
        }
    }
}

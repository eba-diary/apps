using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core.Entities.DataProcessing;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class BaseActionMapping : ClassMapping<BaseAction>
    {
        public BaseActionMapping()
        {
            this.Table("DataAction");

            this.Id(x => x.Id, m =>
            {
                m.Column("Id");
                m.Generator(Generators.Identity);
            });
            Discriminator(x => x.Column("ActionType"));
            this.Property(x => x.ActionGuid, m => m.Column("ActionGuid"));
            this.Property(x => x.Name, m => m.Column("Name"));
            this.Property(x => x.Description, m => m.Column("Description"));
            this.Property(x => x.TargetStorageBucket, m => m.Column("TargetStorageBucket"));
            this.Property(x => x.TargetStoragePrefix, m => m.Column("TargetStoragePrefix"));
            this.Property(x => x.TargetStorageSchemaAware, m => m.Column("TargetStorageSchemaAware"));
            this.Property(x => x.TriggerPrefix, m => m.Column("TriggerPrefix"));
        }
    }
}

using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class BusinessAreaMapping : ClassMapping<BusinessArea>
    {
        public BusinessAreaMapping()
        {
            this.Table("BusinessArea");

            this.Cache(c => c.Usage(CacheUsage.ReadOnly));

            this.Id(x => x.Id, m =>
            {
                m.Column("BusinessArea_Id");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Name, m => m.Column("Name_DSC"));
            this.Property(x => x.AbbreviatedName, m => m.Column("AbbreviatedName_DSC"));
        }
    }
}
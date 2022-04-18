using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class SavedSearchMapping : ClassMapping<SavedSearch>
    {
        public SavedSearchMapping()
        {
            Table("SavedSearch");
            Cache((c) => c.Usage(CacheUsage.ReadWrite));
            Id(x => x.SavedSearchId, x =>
            {
                x.Column("SavedSearchId");
                x.Generator(Generators.Identity);
            });
            Property(x => x.SearchType, x =>
            {
                x.Column("SearchType");
                x.NotNullable(true);
            });
            Property(x => x.SearchName, x => 
            {
                x.Column("SearchName");
                x.NotNullable(true);
            });
            Property(x => x.SearchText, x =>
            {
                x.Column("SearchText");
            });
            Property(x => x.FilterCategoriesJson, x =>
            {
                x.Column("FilterCategoriesJson");
                x.Type(NHibernateUtil.StringClob);
            });
            Property(x => x.AssociateId, x =>
            {
                x.Column("AssociateId");
                x.NotNullable(true);
            });
        }
    }
}

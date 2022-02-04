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

            //ISecurable Mapping
            this.Property((x) => x.IsSecured, (m) => m.Column("IsSecured_IND"));
            this.Property((x) => x.PrimaryOwnerId, (m) => m.Column("PrimaryOwner_ID"));
            this.Property((x) => x.PrimaryContactId, (m) => m.Column("PrimaryContact_ID"));
            this.ManyToOne(x => x.Security, m =>
            {
                m.Column("Security_ID");
                m.ForeignKey("FK_Dataset_Security");
                m.Class(typeof(Security));
                m.Cascade(Cascade.All);
            });

        }
    }
}
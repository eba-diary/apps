using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DataAssetMapping : ClassMapping<DataAsset>
    {
        public DataAssetMapping()
        {
            this.Table("DataAsset");

            this.Cache(c => c.Usage(CacheUsage.ReadWrite));

            this.Id(x => x.Id, m =>
            {
                m.Column("DataAsset_ID");
                m.Generator(Generators.Identity);
            });


            this.Property(x => x.Name, m => m.Column("DataAsset_NME"));
            this.Property(x => x.DisplayName, m => m.Column("Display_NME"));
            this.Property(x => x.ArchLink, m => m.Column("ArchDiagram_URL"));
            this.Property(x => x.DataModelLink, m => m.Column("DataModel_URL"));
            this.Property(x => x.GuideLink, m => m.Column("Guide_URL"));
            this.Property(x => x.Contact, m => m.Column("Contact_EML"));
            this.Property(x => x.Description, m => m.Column("DataAsset_DSC"));
            this.Property(x => x.MetadataRepAssetName, m => m.Column("MetadataRepositoryAsset_NME"));

            this.Bag(x => x.Components, m =>
            {
                m.Cache(c => c.Usage(CacheUsage.ReadOnly));
                m.Inverse(true);
                m.Table("ConsumptionLayerComponent");
                m.Cascade(Cascade.All);
                m.Key(k =>
                {
                    k.Column("DataAsset_ID");
                    k.ForeignKey("FK_ConsumptionLayerComponent_DataAsset");
                });
            }, map => map.OneToMany(a => a.Class(typeof(ConsumptionLayerComponent))));
            this.Bag(x => x.AssetSource, m =>
            {
                m.Cache(c => c.Usage(CacheUsage.ReadOnly));
                m.Inverse(true);
                m.Table("AssetSource");
                m.Cascade(Cascade.All);
                m.Key(k =>
                {
                    k.Column("DataAsset_ID");
                    k.ForeignKey("FK_AssetSource_DataAsset");
                });
            }, map => map.OneToMany(a => a.Class(typeof(AssetSource))));
            this.Bag(x => x.AssetNotifications, m =>
            {
                m.Cache(c => c.Usage(CacheUsage.ReadWrite));
                m.Lazy(CollectionLazy.Lazy);
                m.Inverse(true);
                m.Table("AssetNotifications");
                m.Cascade(Cascade.All);
                m.Key(k =>
                {
                    k.Column("DataAsset_ID");
                    k.ForeignKey("FK_AssetNotifications_DataAsset");
                });
            }, map => map.OneToMany(a => a.Class(typeof(Notification))));


            //ISecurable Mapping
            this.Property((x) => x.IsSecured, (m) => m.Column("IsSecured_IND"));
            this.Property((x) => x.PrimaryContactId, (m) => m.Column("PrimaryContact_ID"));
            this.ManyToOne(x => x.Security, m =>
            {
                m.Column("Security_ID");
                m.ForeignKey("FK_DataAsset_Security");
                m.Class(typeof(Security));
                m.Cascade(Cascade.All);
            });

        }
    }
}

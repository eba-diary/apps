using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class AssetNotificationMapping : ClassMapping<AssetNotifications>
    {
        public AssetNotificationMapping()
        {
            this.Table("AssetNotifications");

            this.Cache(c => c.Usage(CacheUsage.ReadWrite));

            this.Id(x => x.NotificationId, m =>
            {
                m.Column("Notification_ID");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Message, m => m.Column("Message_DSC"));
            this.Property(x => x.CreateUser, m => m.Column("CreateUser"));
            this.Property(x => x.StartTime, m => m.Column("Start_DTM"));
            this.Property(x => x.ExpirationTime, m => m.Column("Expire_DTM"));
            //this.Property(x => x.DataAssetId, m => m.Column("DataAsset_ID"));
            this.Property(x => x.MessageSeverity, m => m.Column("Severity_TYP"));
            this.ManyToOne((x) => x.ParentDataAsset, (m) => {   
                m.Column("DataAsset_ID");
                m.ForeignKey("FK_AssetNotifications_DataAsset");
                m.Class(typeof(DataAsset));
            });

        }
    }
}

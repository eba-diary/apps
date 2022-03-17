using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;
using NHibernate;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class NotificationMapping : ClassMapping<Notification>
    {
        public NotificationMapping()
        {
            this.Table("Notifications");

            this.Id(x => x.NotificationId, m =>
            {
                m.Column("Notification_ID");
                m.Generator(Generators.Identity);
            });

            this.Property(x => x.Message, m => { m.Column("Message_DSC"); m.Type(NHibernateUtil.StringClob);});
            this.Property(x => x.CreateUser, m => m.Column("CreateUser"));
            this.Property(x => x.StartTime, m => m.Column("Start_DTM"));
            this.Property(x => x.ExpirationTime, m => m.Column("Expire_DTM"));
            this.Property(x => x.ParentObject, m => m.Column("Object_ID"));
            this.Property(x => x.MessageSeverity, m => m.Column("Severity_TYP"));
            this.Property(x => x.NotificationType, m => m.Column("NotificationType"));
            this.Property(w => w.Title, m => m.Column("Title"));
            this.Property(w => w.NotificationCategory);
            this.Property(w => w.NotificationSubCategoryReleaseNotes);
            this.Property(w => w.NotificationSubCategoryNews);
        }
    }
}

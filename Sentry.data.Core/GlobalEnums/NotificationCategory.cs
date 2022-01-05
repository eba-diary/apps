using System.ComponentModel;


namespace Sentry.data.Core.GlobalEnums
{
    //NotificationCategory is the category of notification much like Severity is also a category
    //NOTE: the NotificationCategory belons specifically to the EventTypeGroup = BusinessAreaDSC AKA BUSINESSAREA_DSC
    //When SpamFactory evaluates Subscriptions, it will check if the Event.Notification.NotificationCategory == Subscription.EventType Description
    public enum NotificationCategory
    {
        //Descrption HERE is important, therefore the source of it is stored in a GlobalConstant so it only exists one place.
        //It is used to match up to the Table EventType.Description.
        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_RELEASE_NOTES)]
        ReleaseNotes = 1,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_TECH_DOC)]
        TechnicalDocuments = 2,

        [Description(GlobalConstants.EventType.NOTIFICATION_DSC_NEWS)]
        News = 3
    }
}

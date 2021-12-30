using System.ComponentModel;


namespace Sentry.data.Core.GlobalEnums
{
    //NotificationCategory is the category of notification much like Severity
    //NOTE: the NotificationCategory equates to the EventTypeGroup = BusinessAreaDSC AKA BUSINESSAREA_DSC
    //When SpamFactory evaluates Subscriptions, it will check if the Event.Notification.NotificationCategory == Subscription.EventType Description
    public enum NotificationCategory
    {
        [Description("Release Notes")]
        ReleaseNotes = 1,

        [Description("Technical Documentation")]
        TechnicalDocuments = 2,

        [Description("News")]
        News = 3
    }
}

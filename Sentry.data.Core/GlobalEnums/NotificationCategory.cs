using System.ComponentModel;


namespace Sentry.data.Core.GlobalEnums
{
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

namespace Sentry.data.Core
{
    public interface IDscEventTopicHelper
    {
        string GetDSCTopic(DatasetFileConfig config);
        string GetDSCTopic(Dataset dataset);
    }
}

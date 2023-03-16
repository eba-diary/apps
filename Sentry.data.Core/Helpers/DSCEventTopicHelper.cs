using Sentry.Configuration;

namespace Sentry.data.Core.Helpers
{
    public class DscEventTopicHelper : IDscEventTopicHelper
    {
        public string GetDSCTopic(DatasetFileConfig config)
        {
            string topicName = GetDSCTopic(config.ParentDataset);
            return topicName;
        }

        public string GetDSCTopic(Dataset dataset)
        {
            string topicName;
            string dscNamedEnvironment = GetDSCNamedEnvironment();
            bool IsDatasetNamedEnvironmentTypeProd = (dataset.NamedEnvironmentType == GlobalEnums.NamedEnvironmentType.Prod);

            topicName = GetDSCTopic_Internal(dscNamedEnvironment, IsDatasetNamedEnvironmentTypeProd);

            return topicName;
        }

        internal string GetDSCTopic_Internal(string dscNamedEnvironment, bool isDatasetNamedEnvironmentTypeProd)
        {
            string topicName;
            if ((dscNamedEnvironment == GlobalConstants.Environments.QUAL || dscNamedEnvironment == GlobalConstants.Environments.PROD) && !isDatasetNamedEnvironmentTypeProd)
            {
                topicName = GetDSCEventTopicConfig("DSCEventTopic_Confluent_NP");
            }
            else
            {
                topicName = GetDSCEventTopicConfig("DSCEventTopic_Confluent");
            }            

            return topicName;
        }

        internal virtual string GetDSCNamedEnvironment()
        {
            return Config.GetDefaultEnvironmentName().ToUpper();
        }
        
        internal virtual string GetDSCEventTopicConfig(string configName)
        {
            return Config.GetHostSetting(configName);
        }
    }
}

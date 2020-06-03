using Sentry.data.Core;
using StructureMap;

namespace Sentry.data.Infrastructure
{
    public static class KafkaHelper
    {
        public static string GetDSCEventTopic()
        {  
            if (UseConfluent())
            {
                return Configuration.Config.GetHostSetting("DSCEventTopic_Confluent");
            }
            else
            {
                return Configuration.Config.GetSetting("SAIDKey").ToUpper() + "-" + Configuration.Config.GetHostSetting("EnvironmentName").ToUpper() + "-" + Configuration.Config.GetHostSetting("DSCEventTopic").ToUpper();
            }            
        }
        public static bool UseConfluent()
        {
            using (IContainer container = Bootstrapper.Container.GetNestedContainer())
            {
                IDataFeatures dataFeatures = container.GetInstance<IDataFeatures>();

                return dataFeatures.Confluent_Kafka_CLA_1793.GetValue();
            }
        }

        public static string GetKafkaBrokers()
        {
            if (UseConfluent())
            {
                return Configuration.Config.GetHostSetting("BrokerServers_Confluent");
            }
            else
            {
                return Configuration.Config.GetHostSetting("KafkaBootstrapServers");
            }
        }

        public static bool UseSASL()
        {
            return (UseConfluent()) ? bool.Parse(Configuration.Config.GetHostSetting("KafkaSSL_Confluent")) : bool.Parse(Configuration.Config.GetHostSetting("KafkaSSL"));
        }

        public static string GetKerberosServiceName()
        {
            return (UseConfluent()) ? Configuration.Config.GetHostSetting("sasl_kerberos_service_name_confluent") : Configuration.Config.GetHostSetting("sasl_kerberos_service_name");
        }

        public static string GetCertPath()
        {
            return (UseConfluent()) ? Configuration.Config.GetHostSetting("CertPath_Confluent") : Configuration.Config.GetHostSetting("CertPath");
        }
    }    
}

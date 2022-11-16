namespace Sentry.data.Infrastructure
{
    public static class KafkaHelper
    {
        public static string Get_Producer_Topic_For_DSCEvents()
        {  
            return Configuration.Config.GetHostSetting("DSCEventTopic_Confluent");
        }

        public static string GetKafkaBrokers()
        {
            return Configuration.Config.GetHostSetting("BrokerServers_Confluent");
        }

        public static bool UseSASL()
        {
            return bool.Parse(Configuration.Config.GetHostSetting("KafkaSSL_Confluent"));
        }

        public static string GetKerberosServiceName()
        {
            return Configuration.Config.GetHostSetting("sasl_kerberos_service_name_confluent");
        }

        public static string GetCertPath()
        {
            return Configuration.Config.GetHostSetting("CertPath_Confluent");
        }
    }    
}

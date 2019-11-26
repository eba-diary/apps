using System;

namespace Sentry.Messaging.Common
{
    public class KafkaSettings
    {
        public string GroupId { get; set; }
        public string BootstrapServers { get; set; }
        public string TopicName { get; set; }
        public string Environment { get; set; }
        public bool IsSSL { get; set; }
        public bool UseLogging { get; set; }
        public string CertPath { get; set; }
        public int TopicPartitions { get; set; }
        public string KerberosServiceName { get; set; }
        //public Func<AsyncCommandProcessor.AsyncCommandProcessor> StatTracker { get; set; } = () => new EmptyQueueStatCommandProcessor();

        public KafkaSettings(string groupId, 
                            string bootstrapServers, 
                            string topicName, 
                            string env, 
                            bool useLogging, 
                            string certPath, 
                            int partitions,
                            bool isSSL,
                            string kerberosServiceName//, 
                            //,Func<AsyncCommandProcessor.AsyncCommandProcessor> statTracker
                            )
        {
            GroupId = groupId;
            BootstrapServers = bootstrapServers;
            TopicName = topicName;
            Environment = env;
            IsSSL = isSSL;
            UseLogging = useLogging;
            CertPath = certPath;
            TopicPartitions = partitions;
            KerberosServiceName = kerberosServiceName;
            //StatTracker = statTracker;
        }
    }
}

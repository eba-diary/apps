using Sentry.data.Core;
using Sentry.Messaging.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class MetadataProcessorService
    {
        private readonly IDataFeatures dataFeatures;

        public MetadataProcessorService(IDataFeatures dataFeatures)
        {
            this.dataFeatures = dataFeatures;
        }

        public void Run()
        {
            ConsumptionConfig cfg = new ConsumptionConfig();
            cfg.ForceSingleThread = false;
            cfg.UseKillFile = true;
            cfg.KillFileLocation = Configuration.Config.GetHostSetting("GoldenEyeWorkDir") + "MetadataProcesserKill.txt";
            cfg.RunMinutes = null;

            IMessageConsumer<string> consumer;

            consumer = GetKafkamessageConsumer(Configuration.Config.GetHostSetting("MetadataProcessorConsumerGroup"));

            MetadataProcessorProvider service = new MetadataProcessorProvider(consumer, GetMessageHandlers(), cfg);
            service.ConsumeMessages();
        }

        private IList<IMessageHandler<string>> GetMessageHandlers()
        {
            IList<IMessageHandler<string>> handlers = new List<IMessageHandler<string>>
            {
                new HiveMetadataService(),
                new SnowflakeEventService(),
                new SparkConverterEventService(),
                new FileDeleteEventService()
            };

            return handlers;
        }

        private IMessageConsumer<string> GetKafkamessageConsumer(string groupId)
        {
            var topicList = Configuration.Config.GetHostSetting("DSCEventTopic_Confluent");
            if (dataFeatures.CLA4411_Goldeneye_Consume_NP_Topics.GetValue() && !string.IsNullOrEmpty(Configuration.Config.GetHostSetting("DSCEventTopic_Confluent_NP")))
            {
                topicList += "," + Configuration.Config.GetHostSetting("DSCEventTopic_Confluent_NP");
            }

            //domain context needed to retrieve config
            KafkaSettings settings = new KafkaSettings(groupId,
                                            KafkaHelper.GetKafkaBrokers(),
                                            topicList,
                                            Configuration.Config.GetHostSetting("EnvironmentName").ToUpper(),
                                            (Configuration.Config.GetHostSetting("KafkaDebugLogging").ToLower() == "true") ? true : false,
                                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KafkaHelper.GetCertPath()), 
                                            0, /*This is not used for consumers*/
                                            KafkaHelper.UseSASL(),
                                            KafkaHelper.GetKerberosServiceName()
                                            );

            return new MetadataProcessorKafkaConsumer(settings);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;
using Sentry.data.Core;
using System.IO;

namespace Sentry.data.Infrastructure
{
    public class MetadataProcessorService
    {
        
        public void Run()
        {
            ConsumptionConfig cfg = new ConsumptionConfig();
            cfg.ForceSingleThread = false;
            cfg.UseKillFile = true;
            cfg.KillFileLocation = Configuration.Config.GetHostSetting("GoldenEyeWorkDir") + "MetadataProcesserKill.txt";
            cfg.RunMinutes = null;

            IMessageConsumer<string> consumer;

            consumer = GetKafkamessageConsumer(Configuration.Config.GetHostSetting("MetadataProcessorConsumerGroup"));

            Messaging.Common.MetadataProcessorService service = new Messaging.Common.MetadataProcessorService(consumer, GetMessageHandlers(), cfg);
            service.ConsumeMessages();
        }

        private IList<IMessageHandler<string>> GetMessageHandlers()
        {
            IList<IMessageHandler<string>> handlers = new List<IMessageHandler<string>>
            {
                //new HiveMetadataHandler(_dsContext)
                new HiveMetadataService(),
                new S3EventService()
            };

            return handlers;
        }

        private IMessageConsumer<string> GetKafkamessageConsumer(string groupId)
        {
            //domain context needed to retrieve config
            KafkaSettings settings = new KafkaSettings(groupId,
                                            Configuration.Config.GetHostSetting("KafkaBootstrapServers"),
                                            Sentry.data.Infrastructure.TopicHelper.GetDSCEventTopic(),
                                            Configuration.Config.GetHostSetting("EnvironmentName").ToUpper(),
                                            (Configuration.Config.GetHostSetting("KafkaDebugLogging").ToLower() == "true") ? true : false,
                                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Configuration.Config.GetHostSetting("CertPath")), 
                                            0, /*This is not used for consumers*/
                                            (Configuration.Config.GetHostSetting("KafkaSSL").ToLower() == "true") ? true : false,
                                            Configuration.Config.GetHostSetting("sasl_kerberos_service_name")
                                            );

            return new MetadataProcessorKafkaConsumer(settings);
        }
    }
}

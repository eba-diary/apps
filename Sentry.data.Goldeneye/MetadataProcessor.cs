﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;
using Sentry.data.Core;

namespace Sentry.data.Goldeneye
{
    public class MetadataProcessor
    {
        public void Run()
        {
            ConsumptionConfig cfg = new ConsumptionConfig();
            cfg.ForceSingleThread = true;
            cfg.UseKillFile = true;
            cfg.KillFileLocation = Configuration.Config.GetHostSetting("GoldenEyeWorkDir") + "MetadataProcessKill.txt";
            cfg.RunMinutes = 2;

            IMessageConsumer<HiveMetadataEvent> consumer;
            IList<IMessageHandler<HiveMetadataEvent>> handlers = new List<IMessageHandler<HiveMetadataEvent>>();
            IMessageHandler<HiveMetadataEvent> handler;

            handler = GetMessageHandlers();

            consumer = GetKafkamessageConsumer("jcg-dotnet-group-99");

            MetadataProcessorService service = new MetadataProcessorService(consumer, handler, cfg);
            service.ConsumeMessages();
        }

        private IMessageHandler<HiveMetadataEvent> GetMessageHandlers()
        {
            return new HiveMetadataHandler();
            //handlerList.Add(new HiveMetadataHandler());

        }

        private IMessageConsumer<HiveMetadataEvent> GetKafkamessageConsumer(string groupId)
        {
            KafkaSettings settings = new KafkaSettings(groupId, "awe-t-apspml-01.sentry.com:6667,awe-t-apspml-02.sentry.com:6667,awe-t-apspml-03.sentry.com:6667", "data-nrdev-goldeneye-000000", "nrdev", false, "", 3);

            return new MetadataProcessorKafkaConsumer(settings);
        }
    }
}

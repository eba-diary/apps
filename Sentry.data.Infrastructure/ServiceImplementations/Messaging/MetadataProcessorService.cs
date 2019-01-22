using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Messaging.Common;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class MetadataProcessorService : IMetadataProcessorService
    {
        #region Declarations
        private IDatasetContext _dsContext;
        #endregion

        #region Constructor
        public MetadataProcessorService(IDatasetContext dsContext)
        {
            _dsContext = dsContext;
        }
        #endregion


        public void Run()
        {
            ConsumptionConfig cfg = new ConsumptionConfig();
            cfg.ForceSingleThread = false;
            cfg.UseKillFile = true;
            cfg.KillFileLocation = Configuration.Config.GetHostSetting("GoldenEyeWorkDir") + "MetadataProcessKill.txt";
            cfg.RunMinutes = 2;

            IMessageConsumer<string> consumer;

            consumer = GetKafkamessageConsumer("jcg-dotnet-group-99");

            Messaging.Common.MetadataProcessorService service = new Messaging.Common.MetadataProcessorService(consumer, GetMessageHandlers(), cfg);
            service.ConsumeMessages();
        }

        private IList<IMessageHandler<string>> GetMessageHandlers()
        {
            IList<IMessageHandler<string>> handlers = new List<IMessageHandler<string>>
            {
                //new HiveMetadataHandler(_dsContext)
                new HiveMetadataService()
            };

            return handlers;
        }

        private IMessageConsumer<string> GetKafkamessageConsumer(string groupId)
        {
            //ApplicaitonConfiguration config = _dsContext.ApplicationConfigurations.Where(w => w.Application == "MetadataProcessor").FirstOrDefault();

            //domain context needed to retrieve config
            KafkaSettings settings = new KafkaSettings(groupId, "awe-t-apspml-01.sentry.com:6667,awe-t-apspml-02.sentry.com:6667,awe-t-apspml-03.sentry.com:6667", "data-nrdev-goldeneye-000000", "nrdev", false, "", 3);

            return new MetadataProcessorKafkaConsumer(settings);
        }
    }
}

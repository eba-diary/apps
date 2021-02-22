using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Infrastructure;
using Sentry.Messaging.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConsumerTesting
{
    public class Class1
    {

        static void Main(string[] args)
        {
            Logger.LoggingFrameworkAdapter = new Sentry.Common.Logging.Adapters.Log4netAdapter(Config.GetHostSetting("AppLogger"));
            //Call your bootstrapper to initialize your application
            Bootstrapper.Init();

            ConsumptionConfig cfg = new ConsumptionConfig();
            cfg.ForceSingleThread = false;
            cfg.UseKillFile = true;
            //cfg.KillFileLocation = Config.GetHostSetting("GoldenEyeWorkDir") + "MetadataProcesserKill.txt";
            cfg.RunMinutes = null;

            IMessageConsumer<string> consumer;

            consumer = GetKafkamessageConsumer(Config.GetHostSetting("MetadataProcessorConsumerGroup"));

            Sentry.Messaging.Common.MetadataProcessorService service = new Sentry.Messaging.Common.MetadataProcessorService(consumer, GetMessageHandlers(), cfg);
            service.ConsumeMessages();

            Logger.Info("Console App completed successfully.");
        }

        private static IList<IMessageHandler<string>> GetMessageHandlers()
        {
            IList<IMessageHandler<string>> handlers = new List<IMessageHandler<string>>
            {
                //new HiveMetadataHandler(_dsContext)
                new HiveMetadataService(),
                new S3EventService(),
                new DataStepProcessorService()
            };

            return handlers;
        }

        private static IMessageConsumer<string> GetKafkamessageConsumer(string groupId)
        {
            //domain context needed to retrieve config
            KafkaSettings settings = new KafkaSettings(groupId,
                                            KafkaHelper.GetKafkaBrokers(),
                                            KafkaHelper.GetDSCEventTopic(),
                                            Config.GetHostSetting("EnvironmentName").ToUpper(),
                                            bool.Parse(Config.GetHostSetting("KafkaDebugLogging")),
                                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KafkaHelper.GetCertPath()),
                                            0, /*This is not used for consumers*/
                                            KafkaHelper.UseSASL(),
                                            KafkaHelper.GetKerberosServiceName()
                                            );

            return new MetadataProcessorKafkaConsumer(settings);
        }
    }

    public class ConsoleWriteService : IMessageHandler<string>
    {

        #region Constructor
        public ConsoleWriteService()
        {

        }

        public async Task HandleAsync(string msg)
        {
            throw new NotImplementedException();
        }
        public void Handle(string msg)
        {
            using (var Container = Bootstrapper.Container.GetNestedContainer())
            {
                IMessageHandler<string> handler = Container.GetInstance<ConsoleWriteHandler>();
                handler.Handle(msg);
            }
        }

        public bool HandleComplete()
        {
            return true;
        }

        public void Init()
        {
            //do nothing
        }
        #endregion
    }

    public class ConsoleWriteHandler : IMessageHandler<string>
    {
        #region Declarations
        #endregion

        #region Constructor
        public ConsoleWriteHandler() { }
        #endregion

        public async Task HandleAsync(string msg)
        {
            throw new NotImplementedException();
        }

        void IMessageHandler<string>.Handle(string msg)
        {
            try
            {
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Logger.Error($"HiveMetadataHandler failed to process message: Msg:({msg})", ex);
            }
        }

        bool IMessageHandler<string>.HandleComplete()
        {
            Logger.Info("S3EventHandlerComplete");
            return true;
        }

        void IMessageHandler<string>.Init()
        {
            Logger.Info("S3EventHandlerInitialized");
        }


    }
}

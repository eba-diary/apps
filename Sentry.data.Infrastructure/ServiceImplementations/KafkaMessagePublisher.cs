using Confluent.Kafka;
using Sentry.Common.Logging;
using Sentry.data.Core;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sentry.data.Infrastructure.Helpers;
using System.Threading.Tasks;
using Sentry.data.Core.Exceptions;

namespace Sentry.data.Infrastructure
{
    public class KafkaMessagePublisher : IMessagePublisher, IDisposable
    {
        #region "declarations"
        static Producer _producer = null;
        static ISerializingProducer<string, string> _producer_str_str = null;
        private static object _initLocker = new object();
        private static object padlock = new object();
        static private IContainer Container { get; set; }

        #endregion

        #region "Publisher Implementation"

        //Single producer will exist.
        public KafkaMessagePublisher()
        {
            lock (padlock)
            {
                if (_producer == null)
                {
                    Init();
                }
            }
        }

        public static Producer Producer
        {
            get
            {
                lock (padlock)
                {
                    if (_producer == null)
                    {
                        Init();
                    }
                    return _producer;
                }
            }
        }

        private static void Init()
        {
            lock (_initLocker)
            {
                Logger.Info("Initializing goldeneye-producer");
                
                try
                {
                    LogSettings();

                    IList<KeyValuePair<String, Object>> configuration = new List<KeyValuePair<String, Object>>()
                    {
                        { new KeyValuePair<String, Object>("statistics.interval.ms", "60000") },
                        { new KeyValuePair<String, Object>("bootstrap.servers", KafkaHelper.GetKafkaBrokers()) }                        
                    };

                    if (KafkaHelper.UseSASL())
                    {
                        configuration.Add(new KeyValuePair<String, Object>("security.protocol", "sasl_ssl"));
                        configuration.Add(new KeyValuePair<String, Object>("sasl.mechanism", "GSSAPI"));
                        configuration.Add(new KeyValuePair<String, Object>("sasl.kerberos.service.name", KafkaHelper.GetKerberosServiceName()));
                        configuration.Add(new KeyValuePair<String, Object>("ssl.ca.location", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KafkaHelper.GetCertPath())));
                    }

                    //Add kafka debug logging 
                    if ((Configuration.Config.GetHostSetting("KafkaDebugLogging").ToLower() == "true")? true : false)
                    {
                        configuration.Add(new KeyValuePair<String, Object>("debug", "all"));
                    }

                    //Print configuration on start
                    string cfgstr = "Producer Configuration:\r\n";
                    cfgstr += $"KafkaSSL: {KafkaHelper.UseSASL().ToString()}\r\n";
                    cfgstr += $"KafkaDebugLogging: {Configuration.Config.GetHostSetting("KafkaDebugLogging")}";
                    foreach (KeyValuePair<String, Object> itm in configuration)
                    {
                        if (!string.IsNullOrEmpty(cfgstr)) cfgstr += "\r\n";

                        cfgstr += itm.Key + ": " + itm.Value.ToString();
                    }
                    Logger.Info(cfgstr);

                    //Create a generic producer
                    _producer = new Producer(configuration);

                    //Send kafka send log events to kafka
                    _producer.OnLog += _producer_OnLog;

                    //create a producer with string serializer for Key and Value
                    _producer_str_str = _producer.GetSerializingProducer(new Confluent.Kafka.Serialization.StringSerializer(Encoding.UTF8), new Confluent.Kafka.Serialization.StringSerializer(Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    Logger.Fatal("Failed to initialize goldeneye-producer", ex);
                }
            }
        }

        private static void _producer_OnLog(object sender, LogMessage e)
        {
            Sentry.Common.Logging.Logger.Info("DEBUG LOG FROM KAFKA: " + e.Message);
        }
        
        public void Publish(string topic, string key, string value)
        {
            try
            {
                if (_producer_str_str == null)
                {
                    Producer p = Producer;
                }

                Logger.Info($"Publishing message - Topic:{topic} Key:{key} Message:{value}");

                _producer_str_str.ProduceAsync(topic, key, value, new ProducerDeliveryHandler());

            }
            catch (Exception ex)
            {
                Logger.Error($"kafkamessageproducer_publish_failure - topic:{topic}:::key:{key}:::value:{value}", ex);
                throw new KafkaProducerException("Failed to publish message", ex);
            }            
        }

        public async Task PublishAsync(string topic, string key, string value)
        {
            try
            {
                if (_producer_str_str == null)
                {
                    Producer p = Producer;
                }

                Logger.Info($"Publishing message - Topic:{topic} Key:{key} Message:{value}");

                await Task.Factory.StartNew(() => _producer_str_str.ProduceAsync(topic, key, value, new ProducerDeliveryHandler())).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                Logger.Error($"kafkamessageproducer_publish_failure - topic:{topic}:::key:{key}:::value:{value}", ex);
                throw new KafkaProducerException("Failed to publish message", ex);
            }
        }

        public void PublishDSCEvent(string key, string value, string topic = null)
        {
            if (string.IsNullOrEmpty(topic)) { topic = KafkaHelper.Get_Producer_Topic_For_DSCEvents(); }
            Publish(topic, key, value);
        }

        public async Task PublishDSCEventAsync(string key, string value, string topic = null)
        {
            if (string.IsNullOrEmpty(topic)) { topic = KafkaHelper.Get_Producer_Topic_For_DSCEvents(); }
            await PublishAsync(topic, key, value).ConfigureAwait(false);
        }
        #endregion
        
        private static void LogSettings()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Brokers: {KafkaHelper.GetKafkaBrokers()}");
            sb.AppendLine($"UseSASL: {KafkaHelper.UseSASL()}");
            sb.AppendLine($"KerberosServiceName: {KafkaHelper.GetKerberosServiceName()}");
            sb.AppendLine($"KafkaDebugLogging: {Configuration.Config.GetHostSetting("KafkaDebugLogging")}");

            Logger.Debug(sb.ToString());
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_producer != null) _producer.Dispose();
                }

                Sentry.Common.Logging.Logger.Info("Disposing of producer");

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~KafkaMessagePublisher() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #region InternalClasses
        private class QueueProducerStatistic
        {
            public QueueProducerStatistic()
            {
                InternalPublishTime = new InternalPublishTime();
                RoundTripTime = new RoundTripTime();
            }

            public string Environment;
            public string ProducerName;
            public string QueueType;
            public string QueueName;
            public string Server;
            public string Port;
            public string InternalId;
            public InternalPublishTime InternalPublishTime;
            public RoundTripTime RoundTripTime;
        }

        private class InternalPublishTime
        {
            public int MinimumTime;
            public int MaximumTime;
            public int AverageTime;
            public int SumTime;
            public int SamplePoints;
        }

        private class RoundTripTime
        {
            public int MinimumTime;
            public int MaximumTime;
            public int AverageTime;
            public int SumTime;
            public int SamplePoints;
        }
        #endregion
        

        #endregion
    }
}

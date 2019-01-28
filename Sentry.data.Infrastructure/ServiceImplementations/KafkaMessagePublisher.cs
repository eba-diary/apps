using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using StructureMap;
using Sentry.Common.Logging;

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
                Sentry.Common.Logging.Logger.Info("Initializing goldeneye-producer");

                ApplicationConfiguration config = null;

                try
                {
                    //Return configuration from database
                    using (Container = Sentry.data.Infrastructure.Bootstrapper.Container.GetNestedContainer())
                    {
                        IRequestContext reqContext = Container.GetInstance<IRequestContext>();

                        config = reqContext.ApplicaitonConfigurations.Where(w => w.Application == "goldeneye-producer").FirstOrDefault();
                    }
                    IList<KeyValuePair<String, Object>> configuration = new List<KeyValuePair<String, Object>>();

                    //Adding Production options from database
                    foreach (JObject item in config.OptionsObject["ProducerOptions"])
                    {
                        foreach (JProperty prop in item.Properties())
                        {
                            configuration.Add(new KeyValuePair<String, Object>(prop.Name, prop.Value));
                        }
                    }

                    //Add kafka debug logging 
                    if ((bool)config.OptionsObject["DebugLogging"])
                    {
                        configuration.Add(new KeyValuePair<String, Object>("debug", "all"));
                    }

                    //Print configuration on start
                    string cfgstr = "Producer Configuration:";
                    foreach (KeyValuePair<String, Object> itm in configuration)
                    {
                        if (!string.IsNullOrEmpty(cfgstr)) cfgstr += "\r\n";

                        cfgstr += itm.Key + ": " + itm.Value.ToString();
                    }
                    Sentry.Common.Logging.Logger.Info(cfgstr);

                    //Create a generic producer
                    _producer = new Producer(configuration);

                    //Send kafka send log events to kafka
                    _producer.OnLog += _producer_OnLog;
                    //_producer.OnStatistics += _producer_OnStatistics;

                    //create a producer with string serializer for Key and Value
                    _producer_str_str = _producer.GetSerializingProducer(new Confluent.Kafka.Serialization.StringSerializer(Encoding.UTF8), new Confluent.Kafka.Serialization.StringSerializer(Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    Sentry.Common.Logging.Logger.Fatal("Failed to initialize goldeneye-producer", ex);
                }
            }
        }

        private static void _producer_OnLog(object sender, LogMessage e)
        {
            Sentry.Common.Logging.Logger.Info("DEBUG LOG FROM KAFKA: " + e.Message);
        }

        //private static void _producer_OnStatistics(object sender, string e)
        //{
        //    var json = JsonConvert.DeserializeObject<JObject>(e);

        //    JToken brokers_tkn = null;
        //    json.TryGetValue("brokers", out brokers_tkn);

        //    if (brokers_tkn != null && typeof(JObject) == brokers_tkn.GetType())
        //    {
        //        if (brokers_tkn.Children<JProperty>().ToList().Count > 0)
        //        {
        //            IList<JProperty> brokers = brokers_tkn.Children<JProperty>().ToList();
        //            foreach (JProperty jp in brokers)
        //            {
        //                if (jp.Value != null && typeof(JObject) == jp.Value.GetType())
        //                {
        //                    QueueProducerStatistic stats = new QueueProducerStatistic();
        //                    stats.Environment = _environment;
        //                    stats.ProducerName = "GOLDENEYE-PRODUCER";
        //                    stats.QueueType = "KAFKA";
        //                    stats.QueueName = _topicName;

        //                    JObject p = (JObject)jp.Value;
        //                    int nodeId = p.Value<int>("nodeid");
        //                    JObject ilatency = p.Value<JObject>("int_latency");
        //                    JObject rtt = p.Value<JObject>("rtt");
        //                    string fullServer = p.Value<string>("name");

        //                    if (nodeId >= 0 && !string.IsNullOrEmpty(fullServer))
        //                    {
        //                        stats.Server = fullServer.Split(":".ToCharArray()).First();
        //                        stats.Port = fullServer.Split(":".ToCharArray()).Last().Substring(0, 4);
        //                        stats.InternalId = nodeId.ToString();

        //                        stats report microseconds - i want milliseconds
        //                        stats.InternalPublishTime.MinimumTime = ilatency.GetValue("min").Value<int>() / 1000;
        //                        stats.InternalPublishTime.MaximumTime = ilatency.GetValue("max").Value<int>() / 1000;
        //                        stats.InternalPublishTime.AverageTime = ilatency.GetValue("avg").Value<int>() / 1000;
        //                        stats.InternalPublishTime.SumTime = ilatency.GetValue("sum").Value<int>() / 1000;
        //                        stats.InternalPublishTime.SamplePoints = ilatency.GetValue("cnt").Value<int>();

        //                        stats.RoundTripTime.MinimumTime = rtt.GetValue("min").Value<int>() / 1000;
        //                        stats.RoundTripTime.MaximumTime = rtt.GetValue("max").Value<int>() / 1000;
        //                        stats.RoundTripTime.AverageTime = rtt.GetValue("avg").Value<int>() / 1000;
        //                        stats.RoundTripTime.SumTime = rtt.GetValue("sum").Value<int>() / 1000;
        //                        stats.RoundTripTime.SamplePoints = rtt.GetValue("cnt").Value<int>();

        //                        Sentry.Common.Logging.Logger.Info(JsonConvert.SerializeObject(stats));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public void Publish(string topic, string key, string value)
        {
            try
            {
                if (_producer_str_str == null)
                {
                    Producer p = Producer;
                }

                //var task = Producer.ProduceAsync(topic, Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(value));
                var task = _producer_str_str.ProduceAsync(topic, key, value);

                task.Wait(TimeSpan.FromSeconds(10));

                if (task.Exception != null)
                {
                    foreach (Exception e in task.Exception.Flatten().InnerExceptions)
                    {
                        Logger.Error($"Failed to execute kafka producer ProduceAsync", e);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Kafka Producer failed to send message", ex);
            }            
        }

        public void PublishDSCEvent(string key, string value)
        {
            Publish(Sentry.data.Infrastructure.TopicHelper.GetDSCEventTopic(), key, value);
        }
        #endregion

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

using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public class KafkaMessagePublisher : IMessageHandler<IPublishable>, IDisposable
    {
        #region "declarations"
        readonly KafkaSettings _settings;
        Producer<Null, string> _producer = null;
        private readonly object _locker = new object();
        private readonly AsyncCommandProcessor.AsyncCommandProcessor _statTracker;
        private readonly Task _publishTask = null;
        int _lastPartition = 0;
        #endregion

        #region "IMessageHandler Implementation"
        void IMessageHandler<IPublishable>.Init()
        {
            lock (_locker)
            {
                if (_producer == null)
                {
                    IList<KeyValuePair<String, Object>> configuration = new List<KeyValuePair<String, Object>>() {
                                                                                                            { new KeyValuePair<String, Object>("group.id", _settings.GroupId) },
                                                                                                            { new KeyValuePair<String, Object>("statistics.interval.ms", "60000") },
                                                                                                            { new KeyValuePair<String, Object>("queue.buffering.max.ms", 0) },
                                                                                                            { new KeyValuePair<String, Object>("batch.num.messages", 1) },
                                                                                                            { new KeyValuePair<String, Object>("socket.blocking.max.ms", 1) },
                                                                                                            { new KeyValuePair<String, Object>("retries", 0) },
                                                                                                            { new KeyValuePair<String, Object>("socket.nagle.disable", true) },
                                                                                                            { new KeyValuePair<String, Object>("bootstrap.servers", _settings.BootstrapServers) }
                                                                                                       };
                    if (_settings.UseLogging)
                    {
                        configuration.Add(new KeyValuePair<String, Object>("debug", "all"));
                    }

                    if (_settings.IsSSL)
                    {
                        configuration.Add(new KeyValuePair<String, Object>("security.protocol", "sasl_ssl"));
                        configuration.Add(new KeyValuePair<String, Object>("sasl.mechanisms", "GSSAPI"));
                        configuration.Add(new KeyValuePair<String, Object>("sasl.kerberos.service.name", "kafka"));
                        configuration.Add(new KeyValuePair<String, Object>("ssl.ca.location", _settings.CertPath));
                    }

                    string cfgstr = "Producer Configuration:\r\n";
                    cfgstr += "topic name: " + _settings.TopicName;
                    foreach (KeyValuePair<String, Object> itm in configuration)
                    {
                        if (!string.IsNullOrEmpty(cfgstr)) cfgstr += "\r\n";

                        cfgstr += itm.Key + ": " + itm.Value.ToString();
                    }
                    Sentry.Common.Logging.Logger.Info(cfgstr);

                    _producer = new Producer<Null, String>(configuration, null, new Confluent.Kafka.Serialization.StringSerializer(Encoding.UTF8));

                    _producer.OnLog += _producer_OnLog;
                    _producer.OnStatistics += _producer_OnStatistics;
                    _statTracker.StartPolling();
                }
            }
        }

        private void _producer_OnLog(object sender, LogMessage e)
        {
            Sentry.Common.Logging.Logger.Info("DEBUG LOG FROM KAFKA: " + e.Message);
        }

        private void _producer_OnStatistics(object sender, string e)
        {
            var json = JsonConvert.DeserializeObject<JObject>(e);

            JToken brokers_tkn = null;
            json.TryGetValue("brokers", out brokers_tkn);

            if (brokers_tkn != null && typeof(JObject) == brokers_tkn.GetType())
            {
                if (brokers_tkn.Children<JProperty>().ToList().Count > 0)
                {
                    IList<JProperty> brokers = brokers_tkn.Children<JProperty>().ToList();
                    foreach (JProperty jp in brokers)
                    {
                        if (jp.Value != null && typeof(JObject) == jp.Value.GetType())
                        {
                            QueueProducerStatistic stats = new QueueProducerStatistic();
                            stats.Environment = _settings.Environment;
                            stats.ProducerName = _settings.GroupId;
                            stats.QueueType = "KAFKA";
                            stats.QueueName = _settings.TopicName;

                            JObject p = (JObject)jp.Value;
                            int nodeId = p.Value<int>("nodeid");
                            JObject ilatency = p.Value<JObject>("int_latency");
                            JObject rtt = p.Value<JObject>("rtt");
                            string fullServer = p.Value<string>("name");

                            if (nodeId >= 0 && !string.IsNullOrEmpty(fullServer))
                            {
                                stats.Server = fullServer.Split(":".ToCharArray()).First();
                                stats.Port = fullServer.Split(":".ToCharArray()).Last().Substring(0, 4);
                                stats.InternalId = nodeId.ToString();

                                //stats report microseconds - i want milliseconds
                                stats.InternalPublishTime.MinimumTime = ilatency.GetValue("min").Value<int>() / 1000;
                                stats.InternalPublishTime.MaximumTime = ilatency.GetValue("max").Value<int>() / 1000;
                                stats.InternalPublishTime.AverageTime = ilatency.GetValue("avg").Value<int>() / 1000;
                                stats.InternalPublishTime.SumTime = ilatency.GetValue("sum").Value<int>() / 1000;
                                stats.InternalPublishTime.SamplePoints = ilatency.GetValue("cnt").Value<int>();

                                stats.RoundTripTime.MinimumTime = rtt.GetValue("min").Value<int>() / 1000;
                                stats.RoundTripTime.MaximumTime = rtt.GetValue("max").Value<int>() / 1000;
                                stats.RoundTripTime.AverageTime = rtt.GetValue("avg").Value<int>() / 1000;
                                stats.RoundTripTime.SumTime = rtt.GetValue("sum").Value<int>() / 1000;
                                stats.RoundTripTime.SamplePoints = rtt.GetValue("cnt").Value<int>();

                                if (_statTracker.IsStarted()) _statTracker.QueueCommand(stats);
                            }
                        }
                    }
                }
            }
        }

        void IMessageHandler<IPublishable>.Handle(IPublishable msg)
        {
            lock (_locker)
            {
                _producer.ProduceAsync(_settings.TopicName, null, JsonConvert.SerializeObject(msg), _lastPartition, false).Wait();
                _lastPartition += 1;
                if (_lastPartition >= _settings.TopicPartitions) _lastPartition = 0;
            }
        }

        bool IMessageHandler<IPublishable>.HandleComplete()
        {
            return _publishTask == null || _publishTask.IsCompleted;
        }
        #endregion

        #region "constructors"
        public KafkaMessagePublisher(KafkaSettings settings)
        {
            _statTracker = settings.StatTracker.Invoke();
            _settings = settings;
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
        #endregion

        #endregion
    }
}

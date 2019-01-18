using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.Messaging.Common
{
    public abstract class BaseKafkaConsumer<keyT, msgT> : IMessageConsumer<msgT>, IDisposable
    {
        #region "declarations"
        protected readonly KafkaSettings _settings;
        Consumer<keyT, String> _consumer;
        private readonly AsyncCommandProcessor.AsyncCommandProcessor _statTracker;
        bool _stop = false;
        #endregion

        #region IMessageConsumer Implementation
        public event OnMessageReadyHandler<msgT> MessageReady;
        public event OnConsumerStoppedHandler ConsumerStopped;
        public event OnEndOfStreamHandler EndOfStream;
        public event OnSubscriptionReadyHandler SubscriptionReady;


        void IMessageConsumer<msgT>.Close()
        {
            if (_consumer != null) _consumer.Dispose();
        }

        void IMessageConsumer<msgT>.Open()
        {
            IList<KeyValuePair<String, Object>> configuration = new List<KeyValuePair<String, Object>>() {
                                                                                                    { new KeyValuePair<String, Object>("group.id", _settings.GroupId) },
                                                                                                    { new KeyValuePair<String, Object>("bootstrap.servers", _settings.BootstrapServers) },
                                                                                                    { new KeyValuePair<String, Object>("enable.auto.commit", "true") },
                                                                                                    { new KeyValuePair<String, Object>("auto.commit.interval.ms", "500") },
                                                                                                    { new KeyValuePair<String, Object>("statistics.interval.ms", "60000") },
                                                                                                    { new KeyValuePair<String, Object>("default.topic.config", new Dictionary<string, object>()
                                                                                                        {
                                                                                                            { "auto.offset.reset", "smallest" }
                                                                                                        })
                                                                                                    }
                                                                                                };

            if (_settings.IsSSL)
            {
                configuration.Add(new KeyValuePair<String, Object>("security.protocol", "sasl_ssl"));
                configuration.Add(new KeyValuePair<String, Object>("sasl.mechanisms", "GSSAPI"));
                configuration.Add(new KeyValuePair<String, Object>("sasl.kerberos.service.name", "kafka"));
                configuration.Add(new KeyValuePair<String, Object>("ssl.ca.location", _settings.CertPath));
            }

            if (_settings.UseLogging)
            {
                configuration.Add(new KeyValuePair<String, Object>("debug", "all"));
            }

            Logger.Info("Config Setting: topic name = " + _settings.TopicName);

            foreach (var itm in configuration)
            {
                Logger.Info("Config Setting: " + itm.Key + " = " + itm.Value.ToString());
            }

            _consumer = new Confluent.Kafka.Consumer<keyT, String>(configuration, this.GetKeyDeserializer(), new Confluent.Kafka.Serialization.StringDeserializer(Encoding.UTF8));

            _consumer.OnMessage += _consumer_OnMessage;
            _consumer.OnOffsetsCommitted += _consumer_OnOffsetsCommitted;
            _consumer.OnPartitionsAssigned += _consumer_OnPartitionsAssigned;
            _consumer.OnPartitionsRevoked += _consumer_OnPartitionsRevoked;
            _consumer.OnStatistics += _consumer_OnStatistics;
            _consumer.OnLog += _consumer_OnLog;
            _consumer.Subscribe(_settings.TopicName);

            _statTracker.StartPolling();

            Task.Factory.StartNew(() =>
            {
                while (!_stop)
                {
                    _consumer.Poll(100);
                }

                Logger.Debug("Run Completed Beginning RunOff");
                _statTracker.RunOff();
                Logger.Debug("Run Completed Ending RunOff");
            });
        }

        private void _consumer_OnLog(object sender, LogMessage e)
        {
            if (_settings.UseLogging) Logger.Info(e.Message);
        }

        private void _consumer_OnStatistics(object sender, string e)
        {
            var json = JsonConvert.DeserializeObject<JObject>(e);

            JToken topics_tkn = null;
            json.TryGetValue("topics", out topics_tkn);

            if (topics_tkn != null && typeof(JObject) == topics_tkn.GetType())
            {
                JObject topics = (JObject)(topics_tkn);

                if (topics.HasValues)
                {
                    JObject topic = (JObject)((JProperty)topics.First).Value;
                    if (topic != null)
                    {
                        JToken partitions_tkn = null;
                        topic.TryGetValue("partitions", out partitions_tkn);

                        if (partitions_tkn != null)
                        {

                            IList<JProperty> partitions = partitions_tkn.Children<JProperty>().ToList();
                            foreach (JProperty jp in partitions)
                            {
                                if (jp.Value != null && typeof(JObject) == jp.Value.GetType())
                                {
                                    QueueConsumerStatistic stats = new QueueConsumerStatistic();
                                    stats.Environment = _settings.Environment;
                                    stats.QueueName = _settings.TopicName;
                                    stats.QueueType = "KAFKA";


                                    JObject p = (JObject)jp.Value;
                                    int partition = p.Value<int>("partition");
                                    int committedOffset = p.Value<int>("committed_offset");
                                    int loOffset = p.Value<int>("lo_offset");
                                    int hiOffset = p.Value<int>("hi_offset");
                                    int byteSize = p.Value<int>("msgq_bytes");

                                    if (loOffset >= 0)
                                    {
                                        stats.ConsumerName = _settings.GroupId;
                                        stats.TotalMessagesAvailable = hiOffset - loOffset;
                                        stats.MessagesLeftToConsume = hiOffset - committedOffset;
                                        if (stats.MessagesLeftToConsume > stats.TotalMessagesAvailable) stats.MessagesLeftToConsume = stats.TotalMessagesAvailable;
                                        stats.TotalMessageByteSize = byteSize;
                                        stats.QueueSection = partition.ToString();

                                        if (_statTracker.IsStarted()) _statTracker.QueueCommand(stats);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void _consumer_OnPartitionsRevoked(object sender, List<TopicPartition> e)
        {
            try
            {
                _consumer.Unassign();
                SubscriptionReady(this, false);
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Info("Kafka shutdown during unassigned.  Exception Handled: " + ex.Message);
                _stop = true;
            }
            catch (KafkaException ke)
            {
                Logger.Info("Kafka broker handle destroyed. Exception Handled: " + ke.Message);
                _stop = true;
            }

            Logger.Info("KAFKA Consumer Partitions Revoked for Topic " + _settings.TopicName + " " + String.Join(",", e.OfType<Object>().ToArray()));
        }

        private void _consumer_OnPartitionsAssigned(object sender, List<TopicPartition> e)
        {
            try
            {
                _consumer.Assign(e);
                SubscriptionReady(this, true);
                Logger.Info("KAFKA Consumer Partitions Assigned for Topic " + _settings.TopicName + " " + String.Join(",", e.OfType<Object>().ToArray()));
            }
            catch (Exception ex)
            {
                Logger.Info("ERROR when assigning partitions. Stopping: " + ex.ToString());
                _stop = true;
            }
        }

        private void _consumer_OnOffsetsCommitted(object sender, CommittedOffsets e)
        {
            Logger.Info("KAFKA Offset Committed for topic " + _settings.TopicName + " " + e.ToString());
        }

        protected void ProcessMessage(msgT msg)
        {
            MessageReady(this, msg);
        }

        void IMessageConsumer<msgT>.RequestStop()
        {
            try
            {
                //unassign the partitions
                _consumer.Unassign();
            }
            catch (Exception e)
            {
                Logger.Info(e.Message);
            }
            _stop = true;
        }
        #endregion

        #region Must Override
        protected abstract void _consumer_OnMessage(object sender, Message<keyT, string> e);
        protected abstract Confluent.Kafka.Serialization.IDeserializer<keyT> GetKeyDeserializer();
        #endregion

        #region "IDisposable Implementation"
        public void Dispose()
        {
            if (_consumer != null) _consumer.Dispose();
        }
        #endregion

        #region "constructors"
        protected BaseKafkaConsumer(KafkaSettings settings)
        {
            _statTracker = settings.StatTracker.Invoke();
            _settings = settings;
        }

        #endregion
    }
}


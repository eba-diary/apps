using System;
using Confluent.Kafka;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public abstract class BaseKafkaConsumer<keyT> : IMessageConsumer, IDisposable
    {
        #region Declarations
        private readonly string _groupId;
        private readonly string _bootstrapServers;
        private readonly List<string> _topicNames;
        private readonly string _environment;
        private  Consumer<keyT, String> _consumer;
        

        bool _stop = false;
        #endregion

        //public event OnMessageReadyHandler MessageReady;


        protected BaseKafkaConsumer(string groupId, string bootstrapServers, 
            IList<string> topicNames, string environment)
        {
            _groupId = groupId;
            _bootstrapServers = bootstrapServers;
            _topicNames = topicNames.ToList();
            _environment = environment;
        }
        //public void Init()
        //{
        //    var conf = new Confluent.Kafka.Consumer.ConsumerConfig
        //    {
        //        GroupId = "jcg-test-consumer-group-1",
        //        BootstrapServers = "localhost:9092",
        //        a
        //        // Note: The AutoOffsetReset property determines the start offset in the event
        //        // there are not yet any committed offsets for the consumer group for the
        //        // topic/partitions of interest. By default, offsets are committed
        //        // automatically, so in this example, consumption will only start from the
        //        // eariest message in the topic 'my-topic' the first time you run the program.
        //        //AutoOffsetReset = AutoOffsetResetType.Earliest
        //    };

        //    Confluent.Kafka.
        //}

        void IMessageConsumer.Close()
        {
            if (_consumer != null) _consumer.Dispose();
        }

        void IMessageConsumer.Open()
        {
            IList<KeyValuePair<string, Object>> configuration = new List<KeyValuePair<String, Object>>()
            {
                {new KeyValuePair<string, object>("group.id", _groupId) },
                { new KeyValuePair<String, Object>("bootstrap.servers", _bootstrapServers) },
                { new KeyValuePair<String, Object>("enable.auto.commit", "true") },
                { new KeyValuePair<String, Object>("auto.commit.interval.ms", "500") },
                //{ new KeyValuePair<String, Object>("statistics.interval.ms", "60000") }
                { new KeyValuePair<String, Object>("default.topic.config", new Dictionary<string, object>()
                    {
                        { "auto.offset.reset", "smallest" }
                    })
                }
            };

            _consumer = new Confluent.Kafka.Consumer<keyT, String>(configuration, this.GetKeyDeserializer(), new Confluent.Kafka.Serialization.StringDeserializer(Encoding.UTF8));

            _consumer.OnMessage += _consumer_OnMessage;

            _consumer.Subscribe(_topicNames);

            Task.Factory.StartNew(() =>
            {
                while (!_stop)
                {
                    _consumer.Poll(100);
                }
            });

        }

        void IMessageConsumer.RequestStop()
        {
            try
            {
                //Unassign the partitions
                _consumer.Unassign();
            }
            catch (Exception e)
            {
                Logger.Info(e.Message);
            }

            _stop = true;
        }

        //public void ProcessMessage(string msg)
        //{
        //    MessageReady(this, msg);
        //}

        protected abstract void _consumer_OnMessage(object sender, Message<keyT, string> e);
        protected abstract Confluent.Kafka.Serialization.IDeserializer<keyT> GetKeyDeserializer();

        //public static void Init(string applicationName)
        //{
        //    //Confluent.Kafka. 
        //}
        
        public void Dispose()
        {
            if (_consumer != null) _consumer.Dispose();
        }
    }
}

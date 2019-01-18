using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class MetadataConsumerService : BaseKafkaConsumer<Confluent.Kafka.Serialization.StringDeserializer>, IMetadataConsumerService
    {
        private static object _padlock = new object();

        private MetadataConsumerService _consumer;

        private MetadataConsumerService(string groupId, string bootstrapServers,
            IList<string> topicNames, string environment) : base(groupId, bootstrapServers, topicNames, environment)
        {

        }

        public void Init()
        {
            lock (_padlock)
            {
                if(_consumer == null)
                {
                    _consumer = new MetadataConsumerService("1", "", new List<string>() { "", "" }, "");
                }
            }

            //_consumer.

        }

        protected override IDeserializer<StringDeserializer> GetKeyDeserializer()
        {
            return null;
        }

        protected override void _consumer_OnMessage(object sender, Message<StringDeserializer, string> e)
        {
            throw new NotImplementedException();
        }
    }
}

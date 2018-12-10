using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Sentry.data.Infrastructure.ServiceImplementations
{
    public class KafkaConsumer1
    {
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
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class KafkaSettings
    {
        public string GroupId { get; set; }
        public string BootstrapServers { get; set; }
        public string TopicName { get; set; }
        public string Environment { get; set; }
        public bool IsSSL { get; set; }
        public bool UseLogging { get; set; }
        public string CertPath { get; set; }
        public int TopicPartitions { get; set; }
        //public Func<AsyncCommandProcessor.AsyncCommandProcessor> StatTracker { get; set; } = () => new EmptyQueueStatCommandProcessor();

        public KafkaSettings(string groupId,
                            string bootstrapServers,
                            string topicName,
                            string env,
                            bool useLogging,
                            string certPath,
                            int partitions
                            //,Func<AsyncCommandProcessor.AsyncCommandProcessor> statTracker
                            )
        {
            GroupId = groupId;
            BootstrapServers = bootstrapServers;
            TopicName = topicName;
            Environment = env;
            IsSSL = bootstrapServers.Contains(":6668");
            UseLogging = useLogging;
            CertPath = certPath;
            TopicPartitions = partitions;
            //StatTracker = statTracker;
        }
    }
}

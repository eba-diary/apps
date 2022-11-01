using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Tests
{
    public static class MockClasses
    {
        //create mockcreates here to only do it once
        public static ConnectorCreateRequestDto MockConnectorCreateRequestDto()
        {
            ConnectorCreateRequestConfigDto config = new ConnectorCreateRequestConfigDto()
            {
                ConnectorClass = "io.confluent.connect.s3.S3SinkConnector",
                S3Region = "us-east-2",
                TopicsDir = "topics_2",
                FlushSize = "1",
                TasksMax = "1",
                Timezone = "UTC",
                Transforms = "InsertMetadata",
                Locale = "en-US",
                S3PathStyleAccessEnabled = "false",
                FormatClass = "io.confluent.connect.s3.format.json.JsonFormat",
                S3AclCanned = "bucket-owner-full-control",
                TransformsInsertMetadataPartitionField = "kafka_partition",
                ValueConverter = "org.apache.kafka.connect.json.JsonConverter",
                S3ProxyPassword = "nopass",
                KeyConverter = "org.apache.kafka.connect.converters.ByteArrayConverter",
                S3BucketName = "sentry-dlst-qual-droplocation-ae2",
                PartitionDurationMs = "86400000",
                S3ProxyUser = "SV_DATA_S3CON_I_Q_V1",
                S3SseaName = "AES256",
                TransformsInsertMetadataOffsetField = "kafka_offset",
                FileDelim = "_",
                Topics = "Gojira",
                PartitionerClass = "io.confluent.connect.storage.partitioner.TimeBasedPartitioner",
                ValueConverterSchemasEnable = "false",
                TransformsInsertMetadataTimestampField = "kafka_timestamp",
                Name = "S3_Gojira_001",
                StorageClass = "io.confluent.connect.s3.storage.S3Storage",
                RotateScheduleIntervalMs = "86400000",
                PathFormat = "YYYY/MM/dd",
                TimestampExtractor = "Record",
                TransformsInsertMetadataType = "org.apache.kafka.connect.transforms.InsertField$Value",
                S3ProxyUrl = "https://app-proxy-nonprod.sentry.com:8080"
            };

            ConnectorCreateRequestDto request = new ConnectorCreateRequestDto()
            {
                Name = "S3_Gojira_001",
                Config = config
            };

            return request;
        }

        public static ConnectorCreateResponseDto MockConnectorCreateResponseDto(string SuccessStatusCodeDescription)
        {
            ConnectorCreateResponseDto responseDto = new ConnectorCreateResponseDto()
            {
                SuccessStatusCode = true,
                SuccessStatusCodeDescription = $"{SuccessStatusCodeDescription}",
                StatusCode = "201",
                ReasonPhrase = "Created"
            };

            return responseDto;
        }
    }
}

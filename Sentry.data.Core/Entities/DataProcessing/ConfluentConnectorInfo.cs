using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class ConfluentConnectorInfo
    {
        public string name { get; set; }

        [JsonProperty("config")]
        public ConfluentConnectorInfoConfig confluentConnectorInfoConfig { get; set; }
    }

    public class ConfluentConnectorInfoConfig
    {
        [JsonProperty("connector.class")]
        public string ConnectorClass { get; set; }

        [JsonProperty("s3.region")]
        public string S3Region { get; set; }

        [JsonProperty("flush.size")]
        public string FlushSize { get; set; }

        [JsonProperty("tasks.max")]
        public string TasksMax { get; set; }
        public string timezone { get; set; }
        public string transforms { get; set; }
        public string locale { get; set; }

        [JsonProperty("s3.path.style.access.enabled")]
        public string S3PathStyleAccessEnabled { get; set; }

        [JsonProperty("format.class")]
        public string FormatClass { get; set; }

        [JsonProperty("s3.acl.canned")]
        public string S3AclCanned { get; set; }

        [JsonProperty("transforms.InsertMetadata.partition.field")]
        public string TransformsInsertMetadataPartitionField { get; set; }

        [JsonProperty("value.converter")]
        public string ValueConverter { get; set; }

        [JsonProperty("s3.proxy.password")]
        public string S3ProxyPassword { get; set; }

        [JsonProperty("key.converter")]
        public string KeyConverter { get; set; }

        [JsonProperty("s3.bucket.name")]
        public string S3BucketName { get; set; }

        [JsonProperty("partition.duration.ms")]
        public string PartitionDurationMs { get; set; }

        [JsonProperty("s3.proxy.user")]
        public string S3ProxyUser { get; set; }

        [JsonProperty("s3.ssea.name")]
        public string S3SseaName { get; set; }

        [JsonProperty("file.delim")]
        public string FileDelim { get; set; }

        [JsonProperty("transforms.InsertMetadata.offset.field")]
        public string TransformsInsertMetadataOffsetField { get; set; }
        public string topics { get; set; }

        [JsonProperty("partitioner.class")]
        public string PartitionerClass { get; set; }
        public string name { get; set; }

        [JsonProperty("value.converter.schemas.enable")]
        public string ValueConverterSchemasEnable { get; set; }

        [JsonProperty("transforms.InsertMetadata.timestamp.field")]
        public string TransformsInsertMetadataTimestampField { get; set; }

        [JsonProperty("storage.class")]
        public string StorageClass { get; set; }

        [JsonProperty("rotate.schedule.interval.ms")]
        public string RotateScheduleIntervalMs { get; set; }

        [JsonProperty("path.format")]
        public string PathFormat { get; set; }

        [JsonProperty("timestamp.extractor")]
        public string TimestampExtractor { get; set; }

        [JsonProperty("s3.proxy.url")]
        public string S3ProxyUrl { get; set; }

        [JsonProperty("transforms.InsertMetadata.type")]
        public string TransformsInsertMetadataType { get; set; }
    }
}

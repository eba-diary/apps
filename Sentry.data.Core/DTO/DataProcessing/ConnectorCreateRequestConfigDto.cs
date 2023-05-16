﻿using Newtonsoft.Json;

namespace Sentry.data.Core
{
    public class ConnectorCreateRequestConfigDto
    {
        [JsonProperty("connector.class")]
        public string ConnectorClass { get; set; }

        [JsonProperty("s3.region")]
        public string S3Region { get; set; }

        [JsonProperty("topics.dir")]
        public string TopicsDir { get; set; }

        [JsonProperty("flush.size")]
        public string FlushSize { get; set; }

        [JsonProperty("tasks.max")]
        public string TasksMax { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("transforms")]
        public string Transforms { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

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

        [JsonProperty("topics")]
        public string Topics { get; set; }

        [JsonProperty("partitioner.class")]
        public string PartitionerClass { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

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

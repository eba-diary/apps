using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConnectorInfoDto
    {
        public string Name { get; set; }
        public string Type  { get; set; }
        public string ConnectorClass { get; set; }
        public string S3Region { get; set; }
        public string FlushSize { get; set; }
        public string TasksMax { get; set; }
        public string timezone { get; set; }
        public string transforms { get; set; }
        public string locale { get; set; }
        public string S3PathStyleAccessEnabled { get; set; }
        public string FormatClass { get; set; }
        public string S3AclCanned { get; set; }
        public string TransformsInsertMetadataPartitionField { get; set; }
        public string ValueConverter { get; set; }
        public string S3ProxyPassword { get; set; }
        public string KeyConverter { get; set; }
        public string S3BucketName { get; set; }
        public string PartitionDurationMs { get; set; }
        public string S3ProxyUser { get; set; }
        public string S3SseaName { get; set; }
        public string FileDelim { get; set; }
        public string TransformsInsertMetadataOffsetField { get; set; }
        public string topics { get; set; }
        public string PartitionerClass { get; set; }
        public string ValueConverterSchemasEnable { get; set; }
        public string TransformsInsertMetadataTimestampField { get; set; }
        public string StorageClass { get; set; }
        public string RotateScheduleIntervalMs { get; set; }
        public string PathFormat { get; set; }
        public string TimestampExtractor { get; set; }
        public string S3ProxyUrl { get; set; }
        public string TransformsInsertMetadataType { get; set; }
    }
}

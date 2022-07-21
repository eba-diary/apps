using Nest;
using System;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DataFlowMetric
    {
        #region Properties
        [PropertyName("@timestamp")]
        public DateTime QueryMadeDateTime { get; set; }

        [PropertyName("schemaid")]
        public int SchemaId { get; set; }

        [PropertyName("messagevalue")]
        public string EventContents { get; set; }

        [PropertyName("maxexecutionorder")]
        public int MaxExecutionOrder { get; set; }

        [PropertyName("filemodifieddate")]
        public DateTime FileModifiedDateTime { get; set; }

        [PropertyName("originalfilename")]
        public string OriginalFileName { get; set; }

        [PropertyName("datasetid")]
        public int DatasetId { get; set; }

        [PropertyName("executionorder")]
        public int ExecutionOrder { get; set; }

        [PropertyName("dataactionid")]
        public int DataActionId { get; set; }

        [PropertyName("dataflowid")]
        public int DataFlowId { get; set; }

        [PropertyName("partition")]
        public int Partition { get; set; }

        [PropertyName("dataactiontypeid")]
        public int DataActionTypeId { get; set; }

        [PropertyName("messagekey")]
        public string MessageKey  { get; set; }

        [PropertyName("duration")]
        public int Duration { get; set; }

        [PropertyName("offset")]
        public int Offset { get; set; }

        [PropertyName("dataflowname")]
        public string DataFlowName { get; set; }

        [PropertyName("dataflowstepid")]
        public int DataFlowStepId { get; set; }

        [PropertyName("flowexecutionguid")]
        public string FlowExecutionGuid { get; set; }

        [PropertyName("filesize")]
        public int FileSize { get; set; }

        [PropertyName("eventmetricid")]
        public int EventMetricId { get; set; }

        [PropertyName("storagecode")]
        public string StorageCode { get; set; }

        [PropertyName("filecreateddate")]
        public DateTime FileCreatedDateTime { get; set; }

        [PropertyName("runinstanceguid")]
        public string RunInstanceGuid { get; set; }

        [PropertyName("filename")]
        public string FileName { get; set; }

        [PropertyName("saidkeycode")]
        public string SaidKeyCode { get; set; }

        [PropertyName("eventmetriccreateddate")]
        public DateTime EventMetricCreatedDateTime { get; set; }

        [PropertyName("datasetfileid")]
        public int DatasetFileId { get; set; }

        [PropertyName("processstartdate")]
        public DateTime ProcessStartDateTime { get; set; }

        [PropertyName("statuscode")]
        public string StatusCode { get; set; }

        #endregion

    }
}
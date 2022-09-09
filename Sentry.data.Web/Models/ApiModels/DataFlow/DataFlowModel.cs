using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web.Models.ApiModels.Dataflow
{
    public class DataFlowModel
    {   public int Id { get; set; }
        public Guid FlowGuid { get; set; }
        public string SaidKeyCode { get; set; }
        public int DatasetId { get; set; }
        public int SchemaId { get; set; }
        public string Name { get; set; }
        public DateTime CreateDTM { get; set; }
        public string CreatedBy { get; set; }
        public string DFQuestionnaire { get; set; }
        public int IngestionType { get; set; }
        public bool IsCompressed { get; set; }
        public int? CompressionType { get; set; }
        public bool IsPreProcessingRequired { get; set; }
        public int? PreProcessingOption { get; set; }
        public string FlowStorageCode { get; set; }
        /// <summary>
        /// Associated RetrieverJobs which pull data from external sources
        /// </summary>
        public List<int> AssociatedJobs { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }
        public string DeleteIssuer { get; set; }
        public DateTime DeleteIssueDTM { get; set; }
        public string NamedEnvironment { get; set; }
        public NamedEnvironmentType NamedEnvironmentType { get; set; }
        public string TopicName { get; set; }
        public string S3ConnectorName { get; set; }
    }
}

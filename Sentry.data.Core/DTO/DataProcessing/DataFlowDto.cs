﻿using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowDto
    {
        public int Id { get; set; }
        public Guid FlowGuid { get; set; }
        public string Name { get; set; }
        public DateTime CreateDTM { get; set; }
        public string CreatedBy { get; set; }
        public string DFQuestionnaire { get; set; }
        public IngestionType IngestionType { get; set; }
        public List<SchemaMapDto> SchemaMap { get; set; }
        public RetrieverJobDto RetrieverJob { get; set; }
        public bool IsCompressed { get; set; }
        public CompressionJobDto CompressionJob { get; set; }
        public bool IsPreProcessingRequired { get; set; }
        public List<DataFlowPreProcessingTypes> PreProcessingOptions { get; set; }
        public string FlowStorageCode { get; set; }
        public List<int> MappedSchema { get; set; }
        /// <summary>
        /// Associated RetrieverJobs which pull data from external sources
        /// </summary>
        public List<int> AssociatedJobs { get; set; }
    }
}

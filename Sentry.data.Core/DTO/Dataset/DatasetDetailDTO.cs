﻿
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetDetailDto : DatasetDto
    {
        public int Downloads { get; set; }
        public Dictionary<string,string> DatasetFileConfigNames { get; set; }
        public Dictionary<string,string> DatasetScopeTypeNames { get; set; }
        public List<string> DistinctFileExtensions { get; set; }
        public int DatasetFileCount { get; set; }
        public string OriginationCode { get; set; }
        public string DataClassificationDescription { get; set; }
        public int GroupAccessCount { get; set; }
        public List<Tuple<string, List<Tuple<DataFlowDetailDto, List<RetrieverJob>>>>> DataFlows { get; set; }
    }
}

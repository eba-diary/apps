using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class SchemaInfoModel
    {
        public int ConfigId { get; set; }
        public int SchemaId { get; set; }
        public string SchemaEntity_NME { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StorageCode { get; set; }
        public string Format { get; set; }
        public bool CurrentView { get; set; }
        public bool IsInSAS { get; set; }
        public string Delimiter { get; set; }
        public bool HasHeader { get; set; }
        public bool IsTrackableSchema { get; set; }
        public int RevisionCount { get; set; }
        public string HiveTable { get; set; }
        public string HiveDatabase { get; set; }
        public string HiveTableStatus { get; set; }
        public string HiveLocation { get; set; }
        public List<string> Options { get; set; }
        public bool DeleteInd { get; set; }
        public string SnowflakeDatabase { get; set; }
        public string SnowflakeSchema { get; set; }
        public string SnowflakeTable { get; set; }
        public string SnowflakeStatus { get; set; }
        public string ObjectStatus { get; set; }
        public string[] SchemaRootPath { get; set; }
        public bool HasDataFlow { get; set; }
        public string ParquetStorageBucket { get; set; }
        public string ParquetStoragePrefix { get; set; }
    }
}
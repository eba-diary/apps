using Sentry.data.Core;
using System;
using System.Collections.Generic;

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
        //CLA-3306: We are removing IsInSAS from the UI and DSC model. However, the API still exposes this, so we are defaulting to false to maintain the API.
        public bool IsInSAS { get; set; } = false;
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
        public string SnowflakeStage { get; set; }
        public string SnowflakeWarehouse { get; set; }

        public List<string> Validate()
        {
            List<string> results = new List<string>();

            if (string.Equals(Format, GlobalConstants.ExtensionNames.CSV, StringComparison.OrdinalIgnoreCase) && Delimiter != ",")
            {
                results.Add("File Extension CSV and it's delimiter do not match");
            }

            if (string.Equals(Format, GlobalConstants.ExtensionNames.DELIMITED, StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(Delimiter))
            {
                results.Add("File Extension Delimited is missing it's delimiter");
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                results.Add("Configuration Name is required");
            }
            //else if (Name.ToUpper() == "DEFAULT")
            //{
            //    results.Add("Configuration Name cannot be named default");
            //}
            else if (Name.Length > 100)
            {
                results.Add("Configuration Name number of characters cannot be greater than 100");
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                results.Add("Configuration Description is required");
            }
            else if (Description.Length > 2000)
            {
                results.Add("Configuration Description number of characters cannot be greater than 2000");
            }

            return results;
        }
    }
}
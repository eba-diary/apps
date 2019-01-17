using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetDto : BaseEntityDto
    {

        public int DataClassification { get; set; }
        public int OriginationID { get; set; }
        public int DatasetScopeTypeID { get; set; }
        public string SearchCriteria { get; set; }
        public string TargetFileName { get; set; }
        public Boolean CustomDropPath { get; set; }
        public Boolean IsRegexSearch { get; set; }
        public Boolean OverwriteDatasetFile { get; set; }
        public List<int> Configs { get; set; }
        public string ConfigFileName { get; set; }
        public string ConfigFileDesc { get; set; }
        public DataSource DataSource { get; set; } //Needs to be an int?
        public string TargetFileType { get; set; }
        public string Delimiter { get; set; }
        public List<int> DatasetFiles { get; set; }
        public List<int> DatasetScopeType { get; set; }
        public List<string> DropLocations { get; set; }
        public string FileExtension { get; set; }
        public int FileExtensionId { get; set; }
    }
}

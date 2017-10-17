using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;

namespace Sentry.data.Web
{
    public class DatasetFileConfigsModel
    {
        public DatasetFileConfigsModel() { }

        public DatasetFileConfigsModel(DatasetFileConfig dsfc)
        {
            this.ConfigId = dsfc.ConfigId;
            this.DatasetFileConfigID = dsfc.DataFileConfigId;
            this.SearchCriteria = dsfc.SearchCriteria;
            this.TargetFileName = dsfc.TargetFileName;
            this.DropLocationType = dsfc.DropLocationType;
            this.DropPath = dsfc.DropPath;
            this.IsRegexSearch = dsfc.IsRegexSearch;
            this.OverwriteDatasetFile = dsfc.OverwriteDatafile;
            this.VersionsToKeep = dsfc.VersionsToKeep;
            this.FileTypeId = dsfc.FileTypeId;
            this.ConfigFileName = dsfc.Name;
            this.ConfigFileDesc = dsfc.Description;
            this.DatasetId = dsfc.DatasetId;         
        }
        
        public int ConfigId { get; set; }
        public int DatasetFileConfigID { get; set; }
        [Required]
        public string SearchCriteria { get; set; }
        public string TargetFileName { get; set; }
        [Required]
        public string DropLocationType { get; set; }
        [Required]
        public string DropPath { get; set; }
        public Boolean IsRegexSearch { get; set; }
        public Boolean OverwriteDatasetFile { get; set; }
        public int VersionsToKeep { get; set; }
        public int FileTypeId { get; set; }
        [Required]
        public string ConfigFileName { get; set; }
        public string ConfigFileDesc { get; set; }
        public int DatasetId { get; set; }
        public string EditHref
        {
            get
            {
                string href = null;
                href = $"<a href=\"#\" onclick=\"data.ManageConfigs.EditConfig({ConfigId})\">Edit</a>";
                return href;
            }
        }
    }
}
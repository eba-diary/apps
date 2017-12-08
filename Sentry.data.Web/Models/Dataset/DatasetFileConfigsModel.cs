using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DatasetFileConfigsModel
    {
        public DatasetFileConfigsModel() { }

        public DatasetFileConfigsModel(DatasetFileConfig dsfc)
        {
            this.ConfigId = dsfc.ConfigId;
            this.SearchCriteria = dsfc.SearchCriteria;
            this.TargetFileName = dsfc.TargetFileName;
            this.DropLocationType = dsfc.DropLocationType;
            this.DropPath = dsfc.DropPath;
            this.IsRegexSearch = dsfc.IsRegexSearch;
            this.OverwriteDatasetFile = dsfc.OverwriteDatafile;
            this.FileTypeId = dsfc.FileTypeId;
            this.ConfigFileName = dsfc.Name;
            this.ConfigFileDesc = dsfc.Description;
            this.ParentDatasetName = dsfc.ParentDataset.DatasetName;
            this.CreationFreq = dsfc.CreationFreqDesc;
            this.DatasetScopeTypeID = dsfc.DatasetScopeTypeID;
        }
        
        public int ConfigId { get; set; }
        [Required]
        public string SearchCriteria { get; set; }
        public string TargetFileName { get; set; }
        [Required]
        public string DropLocationType { get; set; }
        [Required]
        public string DropPath { get; set; }
        public Boolean IsRegexSearch { get; set; }
        public Boolean OverwriteDatasetFile { get; set; }
        public int FileTypeId { get; set; }

        public string FileType {
            get
            {
                return ((FileType) FileTypeId).ToString();
            }
            set
            {
                FileTypeId = (int) Enum.Parse(typeof(FileType), value); ;
            }
        }

        [Required]
        public string ConfigFileName { get; set; }
        public string ConfigFileDesc { get; set; }
        public int DatasetId { get; set; }
        public string EditHref
        {
            get
            {
                string href = null;
                href = $"<a href = \"#\" onclick=\"data.ManageConfigs.EditConfig({ConfigId})\" class=\"table-row-icon\" title=\"Edit Config File\"><i class='glyphicon glyphicon-edit text-primary'></i></a>";
                return href;
            }
        }
        public string ParentDatasetName { get; set; }

        public string CreationFreq { get; set; }
        public int DatasetScopeTypeID { get; set; }
        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }
        public IEnumerable<SelectListItem> AllFrequencies { get; set; }
        public IEnumerable<SelectListItem> AllDataFileTypes { get; set; }
    }
}
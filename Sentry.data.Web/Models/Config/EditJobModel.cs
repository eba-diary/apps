using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class EditJobModel
    {
        public EditJobModel()
        {

        }

        public EditJobModel(RetrieverJob rj)
        {
            Schedule = rj.Schedule;
            RelativeUri = rj.RelativeUri;
            SearchCriteria = rj.JobOptions.SearchCriteria;
            IsRegexSearch = rj.JobOptions.IsRegexSearch;
            OverwriteDataFile = rj.JobOptions.OverwriteDataFile;
            TargetFileName = rj.JobOptions.TargetFileName;
            CreateCurrentFile = rj.JobOptions.CreateCurrentFile;
            DatasetID = rj.DatasetConfig.ParentDataset.DatasetId;
            DatasetConfigID = rj.DatasetConfig.ConfigId;
            IsSourceCompressed = rj.JobOptions.CompressionOptions.IsCompressed;
            CompressionType = rj.JobOptions.CompressionOptions.CompressionType;
            FileNameExclusionList = rj.JobOptions.CompressionOptions.FileNameExclusionList;
            SelectedDataSource = rj.DataSource.Id;
            JobID = rj.Id;
            IsGeneric = rj.IsGeneric;
        }

        public int JobID { get; set; }
        public Boolean IsGeneric { get; set; }
        public string Schedule { get; set; }
        public int SchedulePicker { get; set; }
        public IEnumerable<SelectListItem> ScheduleOptions { get; set; }
        public string RelativeUri { get; set; }

        public string SearchCriteria { get; set; }

        public Boolean IsRegexSearch { get; set; }

        public Boolean OverwriteDataFile { get; set; }
        public string TargetFileName { get; set; }
        public Boolean CreateCurrentFile { get; set; }

        public int DatasetID { get; set; }
        public int DatasetConfigID { get; set; }

        [DisplayName("Is Source Compressed")]
        public Boolean IsSourceCompressed { get; set; }
        [DisplayName("Compression Type")]
        public string CompressionType { get; set; }
        public IEnumerable<SelectListItem> CompressionTypesDropdown { get; set; }
        public List<string> FileNameExclusionList { get; set; }
        public string NewFileNameExclusionList { get; set; }


        [DisplayName("Data Source")]
        public int SelectedDataSource { get; set; }

        [DisplayName("Source Type")]
        public string SelectedSourceType { get; set; }

        public List<DataSource> AvailableSources { get; set; }

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> SourcesForDropdown { get; set; }
    }
}
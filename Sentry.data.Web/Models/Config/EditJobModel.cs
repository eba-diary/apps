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
            SelectedRequestMethod = rj.JobOptions.HttpOptions.RequestMethod;
            HttpRequestBody = rj.JobOptions.HttpOptions.Body;
        }

        public int JobID { get; set; }
        public Boolean IsGeneric { get; set; }
        public string Schedule { get; set; }
        public IEnumerable<SelectListItem> ScheduleOptions { get; set; }

        [DisplayName("Schedule")]
        public int SchedulePicker { get; set; }

        [DisplayName("Relative URI")]
        public string RelativeUri { get; set; }

        [DisplayName("Https Body")]
        public string HttpRequestBody { get; set; }

        [DisplayName("Search Criteria")]
        public string SearchCriteria { get; set; }

        [DisplayName("Is Search Regex?")]
        public Boolean IsRegexSearch { get; set; }

        [DisplayName("Overwrite Data File")]
        public Boolean OverwriteDataFile { get; set; }

        [DisplayName("Target File Name")]
        public string TargetFileName { get; set; }

        [DisplayName("Create Current File")]
        public Boolean CreateCurrentFile { get; set; }

        public int DatasetID { get; set; }
        public int DatasetConfigID { get; set; }

        [DisplayName("Decompress File?")]
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
        [DisplayName("Request Method")]
        public HttpMethods SelectedRequestMethod { get; set; }
        [DisplayName("Request Body Format")]
        public HttpDataFormat SelectedRequestDataFormat { get; set; }

        public List<DataSource> AvailableSources { get; set; }

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> SourcesForDropdown { get; set; }
        public IEnumerable<SelectListItem> RequestMethodDropdown { get; set; }
        public IEnumerable<SelectListItem> RequestDataFormatDropdown { get; set; }
    }
}
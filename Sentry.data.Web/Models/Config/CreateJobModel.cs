using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class CreateJobModel
    {
        public CreateJobModel()
        {

        }

        public CreateJobModel(int configID, int datasetID)
        {
            this.Schedule = "";
            this.RelativeUri = "";
            this.CompressionType = "";
            this.IsSourceCompressed = false;
            this.DatasetConfigID = configID;
            this.DatasetID = datasetID;

            this.OverwriteDataFile = true;
            this.CreateCurrentFile = false;
            this.TargetFileName = "";

        }

        [Required]
        public string Schedule { get; set; }

        [Required]
        [DisplayName("Schedule")]
        public int SchedulePicker { get; set; }

        [Required]
        [DisplayName("Relative URI")]
        public string RelativeUri { get; set; }

        [DisplayName("Search Criteria")]
        public string SearchCriteria { get; set; }

        [DisplayName("Is Regex Search")]
        public Boolean IsRegexSearch { get; set; }

        [DisplayName("Overwrite Data File")]
        public Boolean OverwriteDataFile { get; set; }

        [DisplayName("Target File Name")]
        public string TargetFileName { get; set; }

        [DisplayName("Create Current File")]
        public Boolean CreateCurrentFile { get; set; }

        public int DatasetID { get; set; }
        public int DatasetConfigID { get; set; }

        [DisplayName("Is Source Compressed")]
        public Boolean IsSourceCompressed { get; set; }
        [DisplayName("Compression Type")]
        public string CompressionType { get; set; }
        public IEnumerable<SelectListItem> CompressionTypesDropdown { get; set; }
        public string NewFileNameExclusionList { get; set; }


        //This is for post backs that fail.
        public List<string> FileNameExclusionList { get; set; }


        [DisplayName("Data Source")]
        public int SelectedDataSource { get; set; }

        [DisplayName("Source Type")]
        public string SelectedSourceType { get; set; }
        public List<DataSource> AvailableSources { get; set; }

        public IEnumerable<SelectListItem> SourceTypesDropdown { get; set; }
        public IEnumerable<SelectListItem> SourcesForDropdown { get; set; }

        public IEnumerable<SelectListItem> SchedulePickerDropdown
        {
            get
            {
                List<SelectListItem> list = new List<SelectListItem>();
                String[] picker = new String[5] { "Hourly", "Daily", "Weekly", "Monthly", "Yearly" };

                int i = 0;
                SelectListItem sli = new SelectListItem()
                {
                    Text = "Pick a Schedule",
                    Selected = true,
                    Disabled = true
                };
                list.Add(sli);
                foreach (String s in picker)
                {
                    sli = new SelectListItem() {
                        Text = s,
                        Value = i.ToString()
                    };
                    list.Add(sli);
                    i++;
                }

                return list;
            }
        }

    }
}
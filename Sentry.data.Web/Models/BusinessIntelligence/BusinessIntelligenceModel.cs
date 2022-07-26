using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class BusinessIntelligenceModel : BaseEntityModel
    {
        public BusinessIntelligenceModel() { }

        public BusinessIntelligenceModel(BusinessIntelligenceDto dto) : base(dto)
        {
            Location = dto.Location;
            FileTypeId = dto.FileTypeId;
            FrequencyId = dto.FrequencyId;
            DatasetBusinessUnitIds = dto.DatasetBusinessUnitIds;
            DatasetFunctionIds = dto.DatasetFunctionIds;
            GetLatest = dto.GetLatest;
            ReportLink = dto.ReportLink;
            BusinessObjectsEnumValue = (int)ReportType.BusinessObjects;
        }

        [Required]
        [MaxLength(1024)]
        [DisplayName("Exhibit Name")]
        public override string DatasetName { get; set; }

        [Required]
        [DisplayName("Report Location")]
        public string Location { get; set; }

        [DisplayName("Exhibit Type")]
        public int FileTypeId { get; set; }

        [Required]
        [DisplayName("Frequency")]
        public int? FrequencyId { get; set; }
        
        [DisplayName("Get Latest")]
        public bool GetLatest { get; set; }

        [DisplayName("Business Unit")]
        public List<int> DatasetBusinessUnitIds { get; set; }

        [DisplayName("Function")]
        public List<int> DatasetFunctionIds { get; set; }
        public string ReportLink { get; set; }

        public int BusinessObjectsEnumValue { get; set; }


        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            switch (this.FileTypeId)
            {
                case (int)ReportType.Tableau:
                    if (!Regex.IsMatch(this.Location.ToLower(), "^https://tableau.sentry.com"))
                    {
                        errors.Add("Tableau exhibits should begin with https://Tableau.sentry.com");
                    }
                    break;
                case (int)ReportType.Excel:
                    if (!Uri.TryCreate(this.Location, UriKind.Absolute, out Uri temp))
                    {
                        errors.Add("Invalid file path.");
                    }
                    break;
                case (int)ReportType.BusinessObjects:
                    if (!Regex.IsMatch(this.Location.ToLower(), "^https://busobj.sentry.com"))
                    {
                        errors.Add("Business Objects exhibits should begin with https://busobj.sentry.com");
                    }
                    break;
            }

            return errors;
        }

    }
}
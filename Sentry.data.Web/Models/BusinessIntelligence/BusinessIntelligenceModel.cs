﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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
        }



        [Required]
        [DisplayName("Report Location")]
        public string Location { get; set; }

        [DisplayName("Exhibit Type")]
        public int FileTypeId { get; set; }

        [Required]
        [DisplayName("Frequency")]
        public int? FrequencyId { get; set; }

        [Required]
        [DisplayName("Business Unit")]
        public List<int> DatasetBusinessUnitIds { get; set; }

        [Required]
        [DisplayName("Function")]
        public List<int> DatasetFunctionIds { get; set; }


        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (!Uri.TryCreate(this.Location, UriKind.Absolute, out Uri temp))
            {
                errors.Add("Invalid report location value");
            }

            switch (this.FileTypeId)
            {
                case (int)ReportType.Tableau:
                    if (!Regex.IsMatch(this.Location.ToLower(), "^https://tableau.sentry.com"))
                    {
                        errors.Add("Tableau exhibits should begin with https://Tableau.sentry.com");
                    }
                    break;
                case (int)ReportType.Excel:
                    if (!Regex.IsMatch(this.Location.ToLower(), "^\\\\\\\\(sentry.com\\\\share\\\\|sentry.com\\\\appfs)"))
                    {
                        errors.Add("Excel exhibits should begin with \\\\Sentry.com\\Share or \\\\Sentry.com\\appfs");
                    }
                    break;
            }

            return errors;
        }

    }
}
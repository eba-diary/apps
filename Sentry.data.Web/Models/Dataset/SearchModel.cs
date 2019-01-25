using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class SearchModel
    {
        public SearchModel(Dataset ds, IAssociateInfoProvider _associateInfoProvider)
        {

            Sentry.Associates.Associate sentryAssociate = _associateInfoProvider.GetAssociateInfo(ds.SentryOwnerName);

            this.Category = ds.DatasetCategory.Name;            
            this.AbbreviatedCategory = (String.IsNullOrWhiteSpace(ds.DatasetCategory.AbbreviatedName)) ? ds.DatasetCategory.Name : ds.DatasetCategory.AbbreviatedName;
            this.DatasetName = ds.DatasetName;
            this.DatasetId = ds.DatasetId;
            this.DatasetDesc = ds.DatasetDesc;
            this.DatasetInformation = ds.DatasetInformation;
            this.SentryOwnerName = Sentry.data.Core.Helpers.DisplayFormatter.FormatAssociateName(sentryAssociate);
            this.DistinctFileExtensions = ds.DatasetFiles.Select(x => Utilities.GetFileExtension(x.FileName).ToLower()).Distinct().ToList();
            this.Frequencies = null;

            if (ds.DatasetFiles.Any())
            {
                this.ChangedDtm = ds.DatasetFiles.Max(x => x.ModifiedDTM).ToShortDateString();
            }
            else
            {
                this.ChangedDtm = ds.ChangedDtm.ToShortDateString();
            }

            this.BannerColor = "categoryBanner-" + ds.DatasetCategory.Color;
            this.BorderColor = "borderSide_" + ds.DatasetCategory.Color;
            this.Color = ds.DatasetCategory.Color;
            this.Type = ds.DatasetType;

            if (ds.DatasetType == "RPT")
            {
                ReportType type = (ReportType)ds.DatasetFileConfigs.First().FileTypeId;
                this.DistinctFileExtensions = new List<string> { type.ToString() };
                this.Tags = ds.Tags.Select(s => s.GetSearchableTag()).ToList();
                Location = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.Location)) ? ds.Metadata.ReportMetadata.Location : null;
                LocationType = (!String.IsNullOrWhiteSpace(ds.Metadata.ReportMetadata.LocationType)) ? ds.Metadata.ReportMetadata.LocationType : null;
                this.UpdateFrequency = (Enum.GetName(typeof(ReportFrequency), ds.Metadata.ReportMetadata.Frequency) != null) ? Enum.GetName(typeof(ReportFrequency), ds.Metadata.ReportMetadata.Frequency) : "Not Specified";
                this.Link = "/BusinessIntelligence/Detail/" + ds.DatasetId;
            }
            else
            {
                this.Link = "/Dataset/Detail/" + ds.DatasetId;
                this.DistinctFileExtensions = ds.DatasetFiles.Select(x => Utilities.GetFileExtension(x.FileName).ToLower()).Distinct().ToList();
            }
        }

        public string Category { get; set; }
        public string AbbreviatedCategory { get; set; }

        public string DatasetName { get; set; }

        public int DatasetId { get; set; }

        public string DatasetDesc { get; set; }

        public string DatasetInformation { get; set; }

        public string SentryOwnerName { get; set; }

        public List<string> DistinctFileExtensions { get; set; }

        public List<string> Frequencies { get; set; }
        public string UpdateFrequency { get; set; }

        public Boolean IsSensitive { get; set; }

        public string ChangedDtm { get; set; }

        public string Color { get; set; }
        public string BannerColor { get; set; }
        public string BorderColor { get; set; }
        public string Location { get; set; }
        public string LocationType { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
        public List<SearchableTag> Tags { get; set; }
        public Boolean IsFavorite { get; set; }
        public Boolean CanEditDataset { get; set; }

    }
}

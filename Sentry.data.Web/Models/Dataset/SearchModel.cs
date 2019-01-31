using Sentry.data.Common;
using Sentry.data.Core;
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

            if (ds.DatasetCategories.Count > 1)
            {
                List<string> catNameList = new List<string>();

                foreach (Category cat in ds.DatasetCategories)
                {
                    // add either Name or Abbreviated Name (if exists)
                    catNameList.Add((!string.IsNullOrWhiteSpace(cat.AbbreviatedName)) ? cat.AbbreviatedName : cat.Name);
                }

                this.Color = "darkgray";
                this.Category = string.Join(", ", catNameList);
                this.AbbreviatedCategory = this.Category;
                this.BannerColor = "categoryBanner-" + this.Color;
                this.BorderColor = "borderSide_" + this.Color;
            }
            else
            {
                this.Color = ds.DatasetCategories.First().Color;
                this.Category = ds.DatasetCategories.First().Name;
                this.AbbreviatedCategory = (!String.IsNullOrWhiteSpace(ds.DatasetCategories.First().AbbreviatedName)) ? ds.DatasetCategories.First().AbbreviatedName : ds.DatasetCategories.First().Name;
                this.BannerColor = "categoryBanner-" + this.Color;
                this.BorderColor = "borderSide_" + this.Color;
            }

            this.Categories = ds.DatasetCategories.Select(x => x.Name).ToList();
            this.CategoryNames = string.Join(", ", this.Categories);
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
            this.ChangedDtm = ds.ChangedDtm.ToShortDateString();
            this.Type = ds.DatasetType;

            if (ds.DatasetType == GlobalConstants.DataEntityTypes.REPORT)
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
        public List<string> Categories { get; set; }
        public string AbbreviatedCategory { get; set; }
        public string CategoryNames { get; set; }

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

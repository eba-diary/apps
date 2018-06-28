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
            this.Category = ds.Category;
            this.DatasetName = ds.DatasetName;
            this.DatasetId = ds.DatasetId;
            this.DatasetDesc = ds.DatasetDesc;
            this.DatasetInformation = ds.DatasetInformation;
            this.SentryOwnerName = _associateInfoProvider.GetAssociateInfo(ds.SentryOwnerName).FullName;
            this.DistinctFileExtensions = ds.DatasetFiles.Select(x => Utilities.GetFileExtension(x.FileName).ToLower()).Distinct().ToList();
            this.Frequencies = null;
            this.ChangedDtm = ds.ChangedDtm.ToShortDateString();

            this.BannerColor = "categoryBanner-" + ds.DatasetCategory.Color;
            this.BorderColor = "borderSide_" + ds.DatasetCategory.Color;
            this.Color = ds.DatasetCategory.Color;
        }

        public string Category { get; set; }

        public string DatasetName { get; set; }

        public int DatasetId { get; set; }

        public string DatasetDesc { get; set; }

        public string DatasetInformation { get; set; }

        public string SentryOwnerName { get; set; }

        public List<string> DistinctFileExtensions { get; set; }

        public List<string> Frequencies { get; set; }

        public Boolean IsSensitive { get; set; }

        public string ChangedDtm { get; set; }

        public string Color { get; set; }
        public string BannerColor { get; set; }
        public string BorderColor { get; set; }

    }
}

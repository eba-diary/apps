using Sentry.data.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class SearchModel
    {
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

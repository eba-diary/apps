using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DatasetTileDto : IFilterSearchable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ObjectStatusEnum Status { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.FAVORITE, hideResultCounts: true)]
        public bool IsFavorite { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.CATEGORY, defaultOpen: true)]
        public string Category { get; set; }
        public string AbbreviatedCategory { get; set; }
        public string Color { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.SECURED, hideResultCounts: true)]
        public bool IsSecured { get; set; }
        public DateTime LastActivityDateTime { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.ORIGIN, hideResultCounts: true)]
        public string OriginationCode { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.ENVIRONMENT, hideResultCounts: true)]
        public string Environment { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.ENVIRONMENTTYPE, defaultOpen:true, hideResultCounts: true)]
        public string EnvironmentType { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.DATASETASSET, hideResultCounts: true)]
        public string DatasetAsset { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.PRODUCERASSET, hideResultCounts: true)]
        public List<string> ProducerAssets { get; set; } = new List<string>();
    }
}

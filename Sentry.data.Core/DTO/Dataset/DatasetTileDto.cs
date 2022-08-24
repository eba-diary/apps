using Sentry.data.Core.GlobalEnums;
using System;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DatasetTileDto : IFilterSearchable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ObjectStatusEnum Status { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.FAVORITE, hideResultCounts:true)]
        public bool IsFavorite { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.CATEGORY, defaultOpen: true)]
        public string Category { get; set; }
        public string AbbreviatedCategory { get; set; }
        public string Color { get; set; }
        [FilterSearchField(FilterCategoryNames.Dataset.SECURED, hideResultCounts: true)]
        public bool IsSecured { get; set; }
        public DateTime LastActivityDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}

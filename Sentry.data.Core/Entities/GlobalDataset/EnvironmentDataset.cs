using Nest;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class EnvironmentDataset
    {
        [PropertyName("datasetid")]
        public int DatasetId { get; set; }

        [PropertyName("datasetdescription")]
        [GlobalSearchField]
        public string DatasetDescription { get; set; }

        [PropertyName("categorycode")]
        [FilterSearchField(FilterCategoryNames.Dataset.CATEGORY, defaultOpen: true)]
        public string CategoryCode { get; set; }

        [PropertyName("namedenvironment")]
        [FilterSearchField(FilterCategoryNames.Dataset.ENVIRONMENT, hideResultCounts: true)]
        public string NamedEnvironment { get; set; }

        [PropertyName("namedenvironmenttype")]
        [FilterSearchField(FilterCategoryNames.Dataset.ENVIRONMENTTYPE, defaultOpen: true, hideResultCounts: true)]
        public string NamedEnvironmentType { get; set; }

        [PropertyName("originationcode")]
        [FilterSearchField(FilterCategoryNames.Dataset.ORIGIN, hideResultCounts: true)]
        public string OriginationCode { get; set; }

        [PropertyName("issecured")]
        [FilterSearchField(FilterCategoryNames.Dataset.SECURED, hideResultCounts: true)]
        public bool IsSecured { get; set; }

        [PropertyName("favoriteuserids")]
        public List<string> FavoriteUserIds { get; set; }

        [PropertyName("environmentschemas")]
        [GlobalSearchNestedField]
        [FilterSearchNestedField]
        public List<EnvironmentSchema> EnvironmentSchemas { get; set; }
    }
}
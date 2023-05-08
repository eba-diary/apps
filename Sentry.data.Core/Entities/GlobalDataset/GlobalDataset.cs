using Nest;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class GlobalDataset : SearchHighlightable
    {
        [PropertyName("globaldatasetid")]
        public int GlobalDatasetId { get; set; }

        [PropertyName("datasetname")]
        [GlobalSearchField(SearchDisplayNames.GlobalDataset.DATASETNAME, 5)]
        public string DatasetName { get; set; }

        [PropertyName("datasetsaidassetcode")]
        [FilterSearchField(FilterCategoryNames.Dataset.DATASETASSET, hideResultCounts: true)]
        public string DatasetSaidAssetCode { get; set; }

        [PropertyName("environmentdatasets")]
        [GlobalSearchField]
        [FilterSearchField]
        public List<EnvironmentDataset> EnvironmentDatasets { get; set; }
    }
}
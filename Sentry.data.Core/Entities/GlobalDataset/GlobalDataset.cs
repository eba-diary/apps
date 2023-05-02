﻿using Nest;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class GlobalDataset
    {
        [PropertyName("globaldatasetid")]
        public int GlobalDatasetId { get; set; }

        [PropertyName("datasetname")]
        [GlobalSearchField(5)]
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
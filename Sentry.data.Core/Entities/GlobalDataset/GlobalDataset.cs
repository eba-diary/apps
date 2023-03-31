using Nest;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class GlobalDataset
    {
        [PropertyName("globaldatasetid")]
        public int GlobalDatasetId { get; set; }
        [PropertyName("datasetname")]
        public string DatasetName { get; set; }
        [PropertyName("datasetasset")]
        public string DatasetAsset { get; set; }
        [PropertyName("datasets")]
        public List<EnvironmentDataset> Datasets { get; set; }
    }
}
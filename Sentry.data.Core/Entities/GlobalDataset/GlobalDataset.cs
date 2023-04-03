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
        [PropertyName("datasetsaidassetcode")]
        public string DatasetSaidAssetCode { get; set; }
        [PropertyName("environmentdatasets")]
        public List<EnvironmentDataset> EnvironmentDatasets { get; set; }
    }
}
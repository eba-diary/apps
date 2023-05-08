using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SearchGlobalDatasetDto
    {
        public int GlobalDatasetId { get; set; }
        public string DatasetName { get; set; }
        public string DatasetSaidAssetCode { get; set; }
        public string DatasetDescription { get; set; }
        public string CategoryCode { get; set; }
        public List<string> NamedEnvironments { get; set; }
        public bool IsSecured { get; set; }
        public bool IsFavorite { get; set; }
        public int TargetDatasetId { get; set; }
    }
}

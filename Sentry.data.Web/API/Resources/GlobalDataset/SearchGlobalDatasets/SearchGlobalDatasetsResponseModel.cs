using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class SearchGlobalDatasetsResponseModel : IResponseModel
    {
        public List<SearchGlobalDatasetResponseModel> GlobalDatasets { get; set; }
    }
}
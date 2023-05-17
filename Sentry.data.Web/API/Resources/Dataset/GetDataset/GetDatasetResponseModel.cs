using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class GetDatasetResponseModel : BaseDatasetResponseModel
    {
        public string SnowflakeWarehouse { get; set; }
        public List<string> SnowflakeDatabases { get; set; }
        public string SnowflakeSchema { get; set; }
    }
}
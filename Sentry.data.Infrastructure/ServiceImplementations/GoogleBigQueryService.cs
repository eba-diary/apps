using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GoogleBigQueryService : IGoogleBigQueryService
    {
        public void UpdateSchemaFields(int datasetId, int schemaId, JArray bigQueryFields)
        {
            throw new NotImplementedException();
        }
    }
}

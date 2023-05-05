using Sentry.data.Core.Entities.Schema.Elastic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISchemaFieldProvider
    {
        Task<List<ElasticSchemaField>> SearchSchemaFieldsAsync(SearchSchemaFieldsDto searchSchemaFieldsDto);
    }
}

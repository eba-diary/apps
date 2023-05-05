using Sentry.data.Core;
using Sentry.data.Core.Entities.Schema.Elastic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SchemaFieldProvider : ISchemaFieldProvider
    {
        private readonly IElasticDocumentClient _elasticDocumentClient;

        public SchemaFieldProvider(IElasticDocumentClient elasticDocumentClient)
        {
            _elasticDocumentClient = elasticDocumentClient;
        }

        public async Task<List<ElasticSchemaField>> SearchSchemaFieldsAsync(SearchSchemaFieldsDto searchSchemaFieldsDto)
        {
            throw new NotImplementedException();
        }
    }
}

using Nest;
using Sentry.data.Core;
using Sentry.data.Core.Entities.Schema.Elastic;
using System.Collections.Generic;
using System.Linq;
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
            BoolQuery searchQuery = searchSchemaFieldsDto.ToSearchQuery<ElasticSchemaField>();

            if (searchSchemaFieldsDto.DatasetIds.Any())
            {
                searchQuery.Filter = new List<QueryContainer>
                {
                    new TermsQuery
                    {
                        Field = Infer.Field<ElasticSchemaField>(x => x.DatasetId),
                        Terms = searchSchemaFieldsDto.DatasetIds.Select(x => (object)x).ToArray()
                    }
                };
            }

            SearchRequest<ElasticSchemaField> searchRequest = new SearchRequest<ElasticSchemaField>()
            {
                Query = searchQuery,
                Size = 10000,
                Highlight = NestHelper.GetHighlight<ElasticSchemaField>()
            };

            ElasticResult<ElasticSchemaField> elasticResult = await _elasticDocumentClient.SearchAsync(searchRequest);

            List<ElasticSchemaField> schemaFields = elasticResult.Hits.ToSearchHighlightedResults();

            return schemaFields;
        }
    }
}

using Sentry.data.Core;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.Schema.Elastic;

namespace Sentry.data.Infrastructure
{
    public class ElasticSchemaSearchProvider
    {
        private readonly IElasticContext _context;
        private readonly int DatasetId;
        private readonly int SchemaId;

        public ElasticSchemaSearchProvider(IElasticContext context, int datasetId, int schemaId)
        {
            _context = context;
            DatasetId = datasetId;
            SchemaId = schemaId;
        }

        public List<ElasticSchemaField> Search(string toSearch)
        {
            if(!String.IsNullOrEmpty(toSearch))
            {
                toSearch = '*' + toSearch.ToLower() + '*';
            }
            Task<ElasticResult<ElasticSchemaField>> result = _context.SearchAsync<ElasticSchemaField>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Filter(
                            bm => bm.Term(p => p.DatasetId, DatasetId),
                            bm => bm.Term(p => p.SchemaId, SchemaId)
                        )
                        .Should(
                            bs => bs.Wildcard(w => w
                                .Field(f => f.Name).Value(toSearch)),
                            bs => bs.Wildcard(w => w
                                .Field(f => f.Description).Value(toSearch)),
                            bs => bs.Wildcard(w => w
                                .Field(f => f.DotNamePath).Value(toSearch))
                            ).MinimumShouldMatch(String.IsNullOrEmpty(toSearch) ? 0 : 1) //If we are searching, we need something to match on (excluding the dataset id or schema id).
                    )
                )
                .Size(2000)
            );
            return (List<ElasticSchemaField>)result.Result.Documents;
        }
    }
}
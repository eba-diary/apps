using Sentry.data.Core;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.Schema.Elastic;

namespace Sentry.data.Infrastructure
{
    public class ElasticSchemaSearchProvider
    {
        private readonly IElasticContext _context;
        private readonly int DatasetId;
        //private int SchemaId;

        public ElasticSchemaSearchProvider(IElasticContext context, int datasetId)
        {
            _context = context;
            DatasetId = datasetId;
        }



        public List<ElasticSchemaField> elasticSearchSchemaFields(string toSearch)
        {
            if (!String.IsNullOrWhiteSpace(toSearch))
            {
                toSearch = '*' + toSearch + '*';
            }
            return (List<ElasticSchemaField>)_context.Search<ElasticSchemaField>(s => s
                .Index("data-schema-column-metadata")
                .AnalyzeWildcard()
                .Query(q => q
                    .Term(p => p.Name, toSearch) || q
                    .Term(p => p.Description, toSearch) || q
                    .Term(p => p.DotNamePath, toSearch)
                )
                .Size(1000)
            ); ;
        }


        /// <summary>
        /// Method to change schema. 
        /// </summary>
        /// <param name="newSchemaId"></param>
        public void changeSchema(int newSchemaId)
        {
            //SchemaId = newSchemaId;
        }

    }
}

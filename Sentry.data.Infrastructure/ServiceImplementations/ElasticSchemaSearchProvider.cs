using Sentry.data.Core;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Entities.Schema.Elastic;
using Sentry.Configuration;

namespace Sentry.data.Infrastructure
{
    public class ElasticSchemaSearchProvider
    {
        private readonly IElasticContext _context;
        private readonly int DatasetId;
        private readonly string Index;
        private int SchemaId;

        public ElasticSchemaSearchProvider(IElasticContext context, int datasetId, int schemaId)
        {
            _context = context;
            DatasetId = datasetId;
            SchemaId = schemaId;
            Index = getElasticIndex();
        }



        public List<ElasticSchemaField> Search(string toSearch)
        {
            Task<ElasticResult<ElasticSchemaField>> result = _context.SearchAsync<ElasticSchemaField>(s => s
                .Index(Index)
                .Query(q => q
                    .Term(p => p.Name, toSearch) || q
                    .Term(p => p.Description, toSearch) || q
                    .Term(p => p.DotNamePath, toSearch)
                )
                .Size(1000)
            );
            return (List<ElasticSchemaField>)result.Result.Documents;
        }


        /// <summary>
        /// Method to change schema. 
        /// </summary>
        /// <param name="newSchemaId"></param>
        public void changeSchema(int newSchemaId)
        {
            //SchemaId = newSchemaId;
        }

        /// <summary>
        /// Method to get the proper index to search on.
        /// 
        /// DEV: Defaults to QUAL data from es-elas-qual. Setting an index up for dev machines isn't worth it.
        /// TEST: Gets data-schema-column-metadata-test from es-elas-qual
        /// QUAL: Gets data-schema-column-metadata from es-elas-qual
        /// PROD: Gets data-schema-column-metadata from es-elas
        /// </summary>
        /// <returns>Name of the index we should query</returns>
        private string getElasticIndex()
        {
            string env = Config.GetDefaultEnvironmentName().ToLower();
            if(env.Equals("test")) return "data-schema-column-metadata-test"
            return "data-schema-column-metadata"
        }

    }
}
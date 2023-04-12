using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ElasticReindexProvider : IReindexProvider
    {
        private readonly IElasticIndexClient _elasticIndexClient;
        private readonly IElasticDocumentClient _elasticDocumentClient;

        public ElasticReindexProvider(IElasticIndexClient elasticIndexClient, IElasticDocumentClient elasticDocumentClient)
        {
            _elasticIndexClient = elasticIndexClient;
            _elasticDocumentClient = elasticDocumentClient;
        }

        public async Task<string> GetCurrentIndexVersionAsync<T>() where T : class
        {
            if (_elasticIndexClient.TryGetAlias<T>(out string alias))
            {
                return await _elasticIndexClient.GetIndexNameByAliasAsync(alias);
            }

            return null;
        }

        public async Task<string> CreateNewIndexVersionAsync(string currentIndex)
        {
            List<string> indexNameParts = currentIndex.Split('-').ToList();
            if (Regex.IsMatch(indexNameParts.Last(), @"^v[1-9]*$"))
            {
                int currentVersion = int.Parse(indexNameParts.Last().Split('v').Last());
                indexNameParts[indexNameParts.Count - 1] = $"v{currentVersion + 1}";
            }
            else
            {
                indexNameParts.Add("v1");
            }

            string newIndexName = string.Join("-", indexNameParts);

            await _elasticIndexClient.CreateIndexAsync(newIndexName);

            return newIndexName;
        }

        public async Task IndexDocumentsAsync<T>(List<T> documents, string indexName) where T : class
        {
            await _elasticDocumentClient.IndexManyAsync(documents, indexName);
        }

        public async Task ChangeToNewIndexAsync<T>(string legacyIndex, string newIndex) where T : class
        {
            _elasticIndexClient.TryGetAlias<T>(out string alias);

            await _elasticIndexClient.AddAliasAsync(newIndex, alias);
            await _elasticIndexClient.DeleteIndexAsync(legacyIndex);
        }
    }
}

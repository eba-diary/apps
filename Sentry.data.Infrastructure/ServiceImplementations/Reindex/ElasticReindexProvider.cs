using Nest;
using Sentry.data.Core;
using Sentry.data.Infrastructure.CherwellService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ElasticReindexProvider : IReindexProvider
    {
        private readonly IElasticClient _elasticClient;

        public ElasticReindexProvider(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<string> GetCurrentIndexVersionAsync<T>() where T : class
        {
            if (_elasticClient.ConnectionSettings.DefaultIndices.TryGetValue(typeof(T), out string alias))
            {
                GetIndexResponse getResponse = await _elasticClient.Indices.GetAsync(alias);
                return getResponse.Indices.Keys.First().Name;
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

            CreateIndexResponse createResponse = await _elasticClient.Indices.CreateAsync(newIndexName, x => x.Settings(s => s.NumberOfShards(3)));
            if (createResponse.IsValid)
            {
                return newIndexName;
            }

            return null;
        }

        public async Task IndexDocumentsAsync<T>(List<T> documents, string indexName) where T : class
        {
            await _elasticClient.IndexManyAsync(documents, indexName);
        }

        public async Task ChangeToNewIndexAsync<T>(string legacyIndex, string newIndex) where T : class
        {
            string alias = _elasticClient.ConnectionSettings.DefaultIndices[typeof(T)];

            PutAliasResponse aliasResponse = await _elasticClient.Indices.PutAliasAsync(newIndex, alias);
            if (aliasResponse.IsValid)
            {
                DeleteIndexResponse deleteResponse = await _elasticClient.Indices.DeleteAsync(legacyIndex);
                if (!deleteResponse.IsValid)
                {
                    throw new ElasticReindexException($"Failed to delete {legacyIndex}. {deleteResponse.DebugInformation}");
                }
            }
            else
            {
                throw new ElasticReindexException($"Failed to add alias to {newIndex}. {aliasResponse.DebugInformation}");
            }
        }
    }
}

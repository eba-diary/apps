using Nest;
using Sentry.data.Core;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ElasticIndexClient : IElasticIndexClient
    {
        private readonly IElasticClient _elasticClient;

        public ElasticIndexClient(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task AddAliasAsync(string indexName, string alias)
        {
            await _elasticClient.Indices.PutAliasAsync(indexName, alias);
        }

        public async Task CreateIndexAsync(string indexName)
        {
            await _elasticClient.Indices.CreateAsync(indexName, x => x.Settings(s => s.NumberOfShards(3)));
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            await _elasticClient.Indices.DeleteAsync(indexName);
        }

        public async Task<string> GetIndexNameByAliasAsync(string alias)
        {
            GetIndexResponse getResponse = await _elasticClient.Indices.GetAsync(alias);
            return getResponse.Indices.Keys.FirstOrDefault()?.Name;
        }

        public bool TryGetAlias<T>(out string alias)
        {
            return _elasticClient.ConnectionSettings.DefaultIndices.TryGetValue(typeof(T), out alias);
        }
    }
}

using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IElasticIndexClient
    {
        bool TryGetAlias<T>(out string alias);
        Task<string> GetIndexNameByAliasAsync(string alias);
        Task CreateIndexAsync(string indexName);
        Task AddAliasAsync(string indexName, string alias);
        Task DeleteIndexAsync(string indexName);
    }
}

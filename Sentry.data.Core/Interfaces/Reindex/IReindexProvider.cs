using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IReindexProvider
    {
        Task<string> GetCurrentIndexVersionAsync<T>() where T : class;
        Task<string> CreateNewIndexVersionAsync(string currentIndex);
        Task IndexDocumentsAsync<T>(List<T> documents, string indexName) where T : class;
        Task ChangeToNewIndexAsync<T>(string legacyIndex, string newIndex) where T : class;
    }
}

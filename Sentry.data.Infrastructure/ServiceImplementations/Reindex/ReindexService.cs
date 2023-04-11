using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class ReindexService<T> : IReindexService where T : class
    {
        private readonly IReindexSource<T> _reindexSource;
        private readonly IReindexProvider _reindexProvider;

        public ReindexService(IReindexSource<T> reindexSource, IReindexProvider reindexProvider)
        {
            _reindexSource = reindexSource;
            _reindexProvider = reindexProvider;
        }

        public async Task ReindexAsync()
        {
            try
            {
                string currentIndex = await _reindexProvider.GetCurrentIndexVersionAsync<T>();
                if (!string.IsNullOrEmpty(currentIndex))
                {
                    Logger.Info($"REINDEX - Reindexing {currentIndex}");

                    string newIndex = await _reindexProvider.CreateNewIndexVersionAsync(currentIndex);
                    if (!string.IsNullOrEmpty(newIndex))
                    {
                        Logger.Info($"REINDEX - Created {newIndex}");

                        while (_reindexSource.TryGetNextDocuments(out List<T> documents))
                        {
                            Logger.Info($"REINDEX - Indexing {documents.Count} documents");

                            await _reindexProvider.IndexDocumentsAsync(documents, newIndex);
                        }

                        Logger.Info($"REINDEX - Completed indexing to {newIndex}");

                        await _reindexProvider.ChangeToNewIndexAsync<T>(currentIndex, newIndex);

                        Logger.Info($"REINDEX - {newIndex} is now being used. {currentIndex} has been deleted");
                    }
                    else
                    {
                        Logger.Warn($"REINDEX - Unable to create {newIndex}");
                    }
                }
                else
                {
                    Logger.Warn($"REINDEX - Unable to get current index version");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"REINDEX - Error while reindexing", ex);
                throw;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DatasetService : IDatasetService
    {
        private static IDatasetService _s3Service;

        public void DeleteDataset(string uniqueKey)
        {
            _s3Service.DeleteDataset(uniqueKey);
        }

        public void Dispose()
        {
            _s3Service.Dispose();
        }

        public T GetById<T>(object id)
        {
            return _s3Service.GetById<T>(id);
        }

        public IEnumerable<T> GetBySqlQuery<T>(string sql, params KeyValuePair<string, object>[] parameters)
        {
            return _s3Service.GetBySqlQuery<T>(sql, parameters);
        }

        public string GetDatasetDownloadURL(string uniqueKey)
        {
            return _s3Service.GetDatasetDownloadURL(uniqueKey);
        }

        public IDictionary<string, string> GetDatasetList(string parentDir = null, bool includeSubDirectories = true)
        {
            return _s3Service.GetDatasetList(parentDir, includeSubDirectories);
        }

        public void UploadDataset(string sourceFilePath, Dataset s3Dataset)
        {
            _s3Service.UploadDataset(sourceFilePath, s3Dataset);
        }
    }
}

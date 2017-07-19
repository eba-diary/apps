using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sentry.data.Core
{
    // JCG TODO: Revisit removing
    //public class DatasetService : IDatasetService
    //{
    //    private static IDatasetService _s3Service;

    //    public void DeleteDataset(string uniqueKey)
    //    {
    //        _s3Service.DeleteDataset(uniqueKey);
    //    }

    //    public void Dispose()
    //    {
    //        _s3Service.Dispose();
    //    }

    //    public T GetById<T>(object id)
    //    {
    //        return _s3Service.GetById<T>(id);
    //    }

    //    public IEnumerable<T> GetBySqlQuery<T>(string sql, params KeyValuePair<string, object>[] parameters)
    //    {
    //        return _s3Service.GetBySqlQuery<T>(sql, parameters);
    //    }

    //    public string GetDatasetDownloadURL(string uniqueKey)
    //    {
    //        return _s3Service.GetDatasetDownloadURL(uniqueKey);
    //    }

    //    public IDictionary<string, string> GetDatasetList(string parentDir = null, bool includeSubDirectories = true)
    //    {
    //        return _s3Service.GetDatasetList(parentDir, includeSubDirectories);
    //    }

    //    public void UploadDataset(string sourceFilePath, Dataset s3Dataset)
    //    {
    //        _s3Service.UploadDataset(sourceFilePath, s3Dataset);
    //    }

    //    public void TransferUtlityUploadStream(string category, string filename, Stream stream)
    //    {
    //        _s3Service.TransferUtlityUploadStream(category, filename, stream);
    //    }

    //    public void TransferUtilityDownload(string baseTargetPath, string folder, string filename, string s3Key)
    //    {
    //        _s3Service.TransferUtilityDownload(baseTargetPath, folder, filename, s3Key);
    //    }
    //}
}

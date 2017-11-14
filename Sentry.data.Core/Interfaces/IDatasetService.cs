using Sentry.Core;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Sentry.data.Core
{
    public interface IDatasetService : IReadableStatelessDomainContext
    {
        IDictionary<string, string> GetDatasetList(string parentDir = null, bool includeSubDirectories = true);

        //Dataset GetDatasetDetails(string uniqueKey);

        string GetDatasetDownloadURL(string uniqueKey);

        /// <summary>
        /// Upload a dataset to S3, pulling directly from the given source file path.  Files size less than
        /// 5MB will use PutObject, larger than 5MB will utilize MultiPartUpload.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="dataSet"></param>
        string UploadDataFile(string sourceFilePath, string targetKey);

        //string UploadDataset_v2(string sourceFilePath, string targetKey);

        string GetObjectPreview(string key);

        void DeleteDataset(string uniqueKey);

        void TransferUtlityUploadStream(string category, string filename, Stream stream);

        void TransferUtlityUploadStream(string key, Stream stream);

        void TransferUtilityDownload(string baseTargetPath, string folder, string filename, string s3Key);

        void DeleteS3key(string key);

        //DatasetFolder GetSubFolderStructure(DatasetFolder parentFolder = null, bool includeSubDirectories = true);

        //IQueryable<Dataset> GetDatasetsByFolderName(string folderName);

        //DatasetFolder GetFolderByUniqueKey(string uniqueKey);

        event EventHandler<TransferProgressEventArgs> OnTransferProgressEvent;

        string MultiPartUpload(string sourceFilePath, string targetKey);

        string GetDatasetDownloadURL(string key, string versionId);
    }

}

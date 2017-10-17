﻿using Sentry.Core;
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

        void UploadDataset(string sourceFilePath, Dataset ds);

        string UploadDataset_v2(string sourceFilePath, string targetKey);

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

        string GetDatasetDownloadURL(string key, string versionId);
    }

}

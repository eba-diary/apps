using Sentry.Core;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
//using Amazon.S3.Model;

namespace Sentry.data.Core
{
    public interface IDatasetService
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

        /// <summary>
        /// Upload file to S3 use a Stream input.  Only utilizes PutObject and limited to 5GB in size.
        /// </summary>
        /// <param name="inputstream"></param>
        /// <param name="targetKey"></param>
        /// <returns></returns>
        string UploadDataFile(Stream inputstream, string targetKey);

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

        Dictionary<string, string> GetObjectMetadata(string key, string versionId);

        Stream GetObject(string key, string versionId);

        string StartUpload(string uniqueKey);

        //CopyPartResponse CopyPart(string dest_Key, int partnum, string source_key, string source_versionId, string uploadId);
    }

}

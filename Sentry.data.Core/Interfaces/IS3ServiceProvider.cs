using System.Collections.Generic;
using System.IO;

namespace Sentry.data.Core
{
    public interface IS3ServiceProvider
    {
        IDictionary<string, string> GetDatasetList(string parentDir = null, bool includeSubDirectories = true);

        /// <summary>
        /// Upload a dataset to S3, pulling directly from the given source file path.  Files size less than
        /// 5MB will use PutObject, larger than 5MB will utilize MultiPartUpload.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="targetKey"></param>
        /// <param name="dataSet"></param>
        string UploadDataFile(string sourceFilePath, string targetBucket, string targetKey, List<KeyValuePair<string, string>> keyValuePairs);

        /// <summary>
        /// Upload a dataset to S3, pulling directly from the given source file path.  Files size less than
        /// 5MB will use PutObject, larger than 5MB will utilize MultiPartUpload. Target bucket will be 
        /// defaulted to DSC root bucket.
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

        string UploadDataFile(Stream inputStream, string targetBucket, string targetKey);

        void TransferUtlityUploadStream(string category, string filename, Stream stream);

        void TransferUtlityUploadStream(string key, Stream stream);

        void TransferUtilityDownload(string baseTargetPath, string folder, string filename, string s3Key, string versionId = null);

        #region Object Deletes
        ObjectKeyVersion MarkDeleted(string key);

        void DeleteS3Key(ObjectKeyVersion keyVersion);

        /// <summary>
        /// Deletes all s3 object keys in list.  Uses specific version id if specified
        /// </summary>
        /// <param name="keyversionids"></param>
        void DeleteMultipleS3Keys(List<ObjectKeyVersion> keyversionids);
        /// <summary>
        /// Deletes all s3 object keys in list.  Will specify null for version id.  In versioned bucket, this will delete all versions of s3 object key.
        /// </summary>
        /// <param name="keys"></param>
        #endregion
        void DeleteMulitpleS3Keys(List<string> keys);

        string MultiPartUpload(string sourceFilePath, string targetBucket, string targetKey, List<KeyValuePair<string, string>> keyValuePairs);

        string GetDatasetDownloadUrl(string key, string bucket = null, string versionId = null, string fileName = null);

        /// <summary>
        /// Returns object metadata for S3 object.
        /// </summary>
        /// <param name="bucket"> Will default to DSC root bucket if null or empty string</param>
        /// <param name="key"></param>
        /// <param name="versionId"> If left null willl perform action on latest version of S3 object</param>
        /// <returns></returns>
        Dictionary<string, string> GetObjectMetadata(string bucket, string key, string versionId = null);

        /// <summary>
        /// Returns object metadata for S3 object.  Will default
        ///   bucket to DSC root bucket
        /// </summary>
        /// <param name="key"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        Dictionary<string, string> GetObjectMetadata(string key, string versionId = null);

        Stream GetObject(string key, string versionId = null);

        string StartUpload(string bucket, string uniqueKey, List<KeyValuePair<string, string>> keyValuePairs);

        List<string> FindObject(string keyPrefix);

        IList<string> ListObjects(string bucket, string prefix, List<KeyValuePair<string, string>> tagList = null);

        List<KeyValuePair<string, string>> GetObjectTags(string bucket, string key, string versionId = null);
        void DeleteParquetFilesByStorageCode(string storageCode);
        /// <summary>
        /// Will delete prefix and all child prefixes. 
        /// </summary>
        /// <param name="prefix"></param>
        void DeleteS3Prefix(string prefix);
        /// <summary>
        /// Will delete prefix and all child prefixes.  For all prefixes in the list.
        /// </summary>
        /// <param name="prefixList"></param>
        void DeleteS3Prefix(List<string> prefixList);

        string CopyObject(string srcBucket, string srcKey, string destBucket, string destKey);
    }

}

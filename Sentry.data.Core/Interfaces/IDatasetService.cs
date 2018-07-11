using Sentry.Core;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Sentry.data.Core
{
    public interface IDatasetService
    {
        IDictionary<string, string> GetDatasetList(string parentDir = null, bool includeSubDirectories = true);

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

        void TransferUtlityUploadStream(string category, string filename, Stream stream);

        void TransferUtlityUploadStream(string key, Stream stream);

        void TransferUtilityDownload(string baseTargetPath, string folder, string filename, string s3Key);

        #region Object Deletes
        ObjectKeyVersion MarkDeleted(string key);

        void DeleteS3Key(ObjectKeyVersion keyversion);

        void DeleteMultipleS3keys(List<ObjectKeyVersion> keyversionids);
        #endregion


        string MultiPartUpload(string sourceFilePath, string targetKey);

        string GetDatasetDownloadURL(string key, string versionId);

        Dictionary<string, string> GetObjectMetadata(string key, string versionId = null);

        Stream GetObject(string key, string versionId);

        string StartUpload(string uniqueKey);

        List<string> FindObject(string keyPrefix);

        IList<string> ListObjects(string bucket, string prefix, List<KeyValuePair<string, string>> tagList);

        List<KeyValuePair<string, string>> GetObjectTags(string bucket, string key, string versionId = null);
    }

}

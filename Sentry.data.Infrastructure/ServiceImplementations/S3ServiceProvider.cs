using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Sentry.data.Core;
using Sentry.NHibernate;
using NHibernate;

namespace Sentry.data.Infrastructure
{
    public class S3ServiceProvider : NHReadableStatelessDomainContext, IDatasetService
    {
        public S3ServiceProvider(IStatelessSession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<S3ServiceProvider>();

        }

        private static Amazon.S3.IAmazonS3 _s3client = null;

        private Amazon.S3.IAmazonS3 S3Client
        {
            get
            {
                if (null == _s3client)
                {
                    // instantiate a new shared client
                    // TODO: move this all to the config(s)...
                    AWSConfigsS3.UseSignatureVersion4 = true;
                    AmazonS3Config s3config = new AmazonS3Config();
                    // s3config.RegionEndpoint = RegionEndpoint.GetBySystemName("us-east-1");
                    s3config.RegionEndpoint = RegionEndpoint.GetBySystemName(Configuration.Config.GetSetting("AWSRegion"));
                    s3config.UseHttp = true;
                    s3config.ProxyHost = "webproxy.sentry.com";
                    s3config.ProxyPort = 80;
                    s3config.ProxyCredentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    string awsAccessKey = Configuration.Config.GetSetting("AWSAccessKey");
                    string awsSecretKey = Configuration.Config.GetSetting("AWSSecretKey");
                    _s3client = new AmazonS3Client(awsAccessKey, awsSecretKey, s3config);
                }
                return _s3client;
            }
        }

        /// <summary>
        /// Get a presigned URL for the dataset in question.
        /// This can be loaded straight into a client browser in order to support faster downloads of large files.
        /// </summary>
        public string GetDatasetDownloadURL(string uniqueKey)
        {
            //var bucketName = Configuration.Config.GetSetting("AWSRootBucket");
            //return S3Client.GetObjectStream(bucketName, uniqueKey, null);

            GetPreSignedUrlRequest req = new GetPreSignedUrlRequest()
            {
                BucketName = Configuration.Config.GetSetting("AWSRootBucket"),
                Key = uniqueKey,
                Expires = DateTime.Now.AddMinutes(2)
            };
            string url = S3Client.GetPreSignedURL(req);
            return url;
        }

        /// <summary>
        /// Upload a dataset to S3, pulling directly from the given source file path
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="dataSet"></param>
        public void UploadDataset(string sourceFilePath, Dataset dataSet)
        {
            PutObjectRequest poReq = new PutObjectRequest();
            poReq.FilePath = sourceFilePath;
            //poReq.BucketName = dataSet.Bucket;
            poReq.Key = dataSet.S3Key;
            System.IO.FileInfo fInfo = new System.IO.FileInfo(sourceFilePath);
            poReq.Metadata.Add("FileName", fInfo.Name);
            poReq.Metadata.Add("Description", dataSet.DatasetDesc);
            //poReq.Metadata.Add("DetailDesc", dataSet.DetailDescription);
            PutObjectResponse poRsp = S3Client.PutObject(poReq);
            if (poRsp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error attempting to upload dataset to S3: " + poRsp.HttpStatusCode);
            }
        }

        public string StartUpload(string uniqueKey)
        {
            InitiateMultipartUploadRequest mReq = new InitiateMultipartUploadRequest();
            mReq.BucketName = Configuration.Config.GetSetting("AWSRootBucket");
            mReq.Key = uniqueKey;
            InitiateMultipartUploadResponse mRsp = S3Client.InitiateMultipartUpload(mReq);
            return mRsp.UploadId;
        }

        public void UploadPart(string uniqueKey, string sourceFilePath, long filePosition, long partSize, int partNumber, bool isLastPart)
        {
            UploadPartRequest uReq = new UploadPartRequest();
            uReq.BucketName = Configuration.Config.GetSetting("AWSRootBucket");
            uReq.Key = uniqueKey;
            //uReq.FilePath = sourceFilePath;
            uReq.FilePosition = filePosition;
            uReq.PartSize = partSize;
            uReq.PartNumber = partNumber;
            uReq.IsLastPart = isLastPart;
            uReq.InputStream = new System.IO.FileStream(sourceFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            uReq.StreamTransferProgress = new EventHandler<Amazon.Runtime.StreamTransferProgressArgs>(UploadProgressEventCallbackHandler);
            UploadPartResponse uRsp = S3Client.UploadPart(uReq);
            if (uRsp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error attempting to upload dataset to S3: " + uRsp.HttpStatusCode);
            }
        }

        public static void UploadProgressEventCallbackHandler(object sender, Amazon.Runtime.StreamTransferProgressArgs e)
        {
            
        }

        public string StopUpload(string uniqueKey, string uploadId)
        {
            CompleteMultipartUploadRequest cReq = new CompleteMultipartUploadRequest();
            cReq.BucketName = Configuration.Config.GetSetting("AWSRootBucket");
            cReq.Key = uniqueKey;
            cReq.UploadId = uploadId;
            CompleteMultipartUploadResponse mRsp = S3Client.CompleteMultipartUpload(cReq);
            return mRsp.ETag;
        }

        /// <summary>
        /// Delete a dataset from S3
        /// </summary>
        /// <param name="uniqueKey"></param>
        public void DeleteDataset(string uniqueKey)
        {
            DeleteObjectRequest doReq = new DeleteObjectRequest();
            doReq.BucketName = Configuration.Config.GetSetting("AWSRootBucket");
            doReq.Key = uniqueKey;
            DeleteObjectResponse doRsp = S3Client.DeleteObject(doReq);
            if (doRsp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error attempting to delete dataset from S3: " + doRsp.HttpStatusCode);
            }
        }

        ///// <summary>
        ///// Retrieve dataset details from S3 using its Etag (not to be confused with its S3 Key).
        ///// </summary>
        ///// <param name="uniqueKey"></param>
        ///// <returns></returns>
        //public Dataset GetDatasetDetails(string uniqueKey)
        //{
        //    GetObjectMetadataRequest s3req = new GetObjectMetadataRequest();
        //    s3req.BucketName = Configuration.Config.GetSetting("AWSRootBucket");
        //    s3req.Key = uniqueKey;
        //    //s3req.EtagToMatch = tagKey;
        //    GetObjectMetadataResponse s3rsp = S3Client.GetObjectMetadata(s3req);
        //    List<DatasetMetadata> md = new Dictionary<string, string>();
        //    md.Add("S3 - ETag", s3rsp.ETag);
        //    md.Add("S3 - Size", s3rsp.ContentLength.ToString());
        //    md.Add("S3 - Last Modified", s3rsp.LastModified.ToShortDateString() + " " + s3rsp.LastModified.ToShortTimeString());
        //    foreach (string mdKey in s3rsp.Metadata.Keys)
        //    {
        //        md.Add(mdKey.Replace("x-amz-meta-", ""), s3rsp.Metadata[mdKey]);
        //    }
        //    string fileName = s3req.Key.Substring(s3req.Key.LastIndexOf("/")).Replace("/", "");
        //    string summaryDesc = "";
        //    string detailDesc = "";
        //    s3rsp.ResponseMetadata.Metadata.TryGetValue("SummaryDesc", out summaryDesc);
        //    s3rsp.ResponseMetadata.Metadata.TryGetValue("x-amz-meta-desc", out detailDesc);
        //    if (summaryDesc == null || summaryDesc.Length == 0) summaryDesc = "<none>";
        //    if (detailDesc == null || detailDesc.Length == 0) detailDesc = "<none>";
        //    String categoryName = uniqueKey.Substring(0, uniqueKey.IndexOf("/"));
        //    Dataset rspDS = new Dataset(
        //        999, 
        //        categoryName,
        //        fileName,
        //        summaryDesc,
        //        "John Schneider",
        //        "John Schneider",
        //        "John Schneider",
        //        "E",
        //        //".txt",
        //        DateTime.Now.AddDays(-2),
        //        s3rsp.LastModified,
        //        DateTime.Now,
        //        "Weekly",
        //        (int)s3rsp.ContentLength,
        //        1,
        //        uniqueKey,
        //        md
        //    );
        //    //    s3rsp.ETag,
        //    //    uniqueKey, 
        //    //    s3req.BucketName, 
        //    //    fileName,
        //    //    s3rsp.LastModified,
        //    //    summaryDesc,
        //    //    detailDesc,
        //    //    s3req.BucketName + "/" + uniqueKey,
        //    //    categoryName,
        //    //    md);
        //    return rspDS;
        //}

        /// <summary>
        /// Get list of datasets currently on S3, within the given parentDir
        /// </summary>
        /// <param name="parentDir"></param>
        /// <param name="includeSubDirectories"></param>
        /// <returns></returns>
        public IDictionary<string, string> GetDatasetList(string parentDir = "sentry-dataset-allaccess-poc", bool includeSubDirectories = true)
        {
            // this will include folders in addition to data sets...
            if (parentDir == null)
            {
                parentDir = Configuration.Config.GetSetting("AWSRootBucket");
            }
            Dictionary<string, string> dsList = new Dictionary<string, string>();
            ListObjectsRequest lbReq = new ListObjectsRequest();
            lbReq.BucketName = parentDir;
            ListObjectsResponse lbRsp = null;
            do
            {   // get list of ojbects
                lbRsp = S3Client.ListObjects(lbReq);
                foreach (S3Object s3o in lbRsp.S3Objects)
                {
                    if (!s3o.Key.EndsWith("/") && s3o.Size > 0)
                    {
                        dsList.Add(s3o.Key, s3o.ETag);
                    }
                }
                if (lbRsp.IsTruncated)
                {
                    lbReq.Marker = lbRsp.NextMarker;
                }
                else
                {
                    lbReq = null;
                }

            } while (lbReq != null);

            return dsList;
        }

        ///// <summary>
        ///// Retrieve dataset details from S3 using its Etag (not to be confused with its S3 Key).
        ///// </summary>
        ///// <param name="uniqueKey"></param>
        ///// <returns></returns>
        //public DatasetFolder GetFolderByUniqueKey(string uniqueKey)
        //{
        //    ListObjectsRequest s3req = new ListObjectsRequest();
        //    s3req.BucketName = Configuration.Config.GetSetting("AWSRootBucket");
        //    s3req.Prefix = uniqueKey;
        //    ListObjectsResponse s3rsp = S3Client.ListObjects(s3req);

        //    //GetObjectMetadataRequest s3req = new GetObjectMetadataRequest();
        //    //s3req.BucketName = Configuration.Config.GetSetting("AWSRootBucket");
        //    //s3req.Key = uniqueKey;
        //    //GetObjectMetadataResponse s3rsp = S3Client.GetObjectMetadata(s3req);
        //    //string fileName = s3req.Key.Substring(s3req.Key.LastIndexOf("/")).Replace("/", "");
        //    //string datasetDecription = "";
        //    //s3rsp.ResponseMetadata.Metadata.TryGetValue("x-amz-meta-desc", out datasetDecription);
        //    //if (datasetDecription == null) datasetDecription = "";
        //    //DatasetFileVersion rspDS = new DatasetFileVersion(s3rsp.ETag, uniqueKey, s3req.BucketName, fileName, summaryDesc, detailDesc, s3req.BucketName + "/" + uniqueKey);

        //    S3Object thisFolderObj = s3rsp.S3Objects.Where(s => s.Key == uniqueKey).FirstOrDefault();
        //    // add this folder to parent
        //    string nameStub = thisFolderObj.Key.TrimEnd("/".ToCharArray());
        //    DatasetFolder dsf = new DatasetFolder(
        //        thisFolderObj.BucketName,
        //        thisFolderObj.Key,
        //        thisFolderObj.ETag,
        //        nameStub.Contains("/") ?
        //            nameStub.Substring(nameStub.LastIndexOf("/")).Replace("/", "") : nameStub,
        //        null);

        //    // populate files...
        //    S3ObjectToDatasetFolder(s3rsp.S3Objects, dsf);

        //    // add metadata
        //    Dictionary<string, string> md = new Dictionary<string, string>();
        //    foreach (string mdKey in s3rsp.ResponseMetadata.Metadata.Keys)
        //    {
        //        if (mdKey == "x-amz-meta-desc")
        //        {
        //            string datasetDecription = "";
        //            s3rsp.ResponseMetadata.Metadata.TryGetValue("x-amz-meta-desc", out datasetDecription);
        //            if (datasetDecription == null) datasetDecription = "";
        //            dsf.Description = datasetDecription;
        //        } else
        //        {
        //            md.Add(mdKey.Replace("x-amz-meta-", ""), s3rsp.ResponseMetadata.Metadata[mdKey]);
        //        }
        //    }
        //    dsf.Metadata = md;

        //    return dsf;
        //}

        ///// <summary>
        ///// Retrieve files from S3 in a file heirarchy struture
        ///// </summary>
        ///// <param name="parentFolder"></param>
        ///// <param name="includeSubDirectories"></param>
        ///// <returns></returns>
        //public DatasetFolder GetSubFolderStructure(DatasetFolder parentFolder = null, bool includeSubDirectories = true)
        //{
        //    DatasetFolder currentFolder = parentFolder;
        //    if (null == currentFolder) {
        //        currentFolder = new DatasetFolder("", "root", "", null);
        //    }

        //    ListObjectsResponse rsp = null;
        //    if (null == parentFolder) {
        //        string rootBucket = Configuration.Config.GetSetting("AWSRootBucket");
        //        rsp = S3Client.ListObjects(rootBucket);
        //    } else {
        //        rsp = S3Client.ListObjects(parentFolder.FullName);
        //    }

        //    // this will recurseively populate all subfolders and children...
        //    S3ObjectToDatasetFolder(rsp.S3Objects, currentFolder);

        //    return currentFolder;
        //}

        ///// <summary>
        ///// Get dataset meta-info for datasets logically residing in the given folder name
        ///// </summary>
        ///// <param name="folderName"></param>
        ///// <returns></returns>
        //public IQueryable<Dataset> GetDatasetsByFolderName(string folderName = null)
        //{
        //    // this will include data sets in addition to folders...
        //    List<Dataset> dsList = new List<Dataset>();
        //    ListObjectsResponse rsp = S3Client.ListObjects(folderName);
        //    foreach (S3Object s3o in rsp.S3Objects) {
        //        // if s3o key appears anywhere else in the output, then s3o is a DatasetFolder
        //        IEnumerable<S3Object> keysContainingThisKey = rsp.S3Objects.Where(s3oChild => s3oChild.Key.Contains(s3o.Key + "/"));
        //        if (keysContainingThisKey.Count() > 0) {
        //            // this is a folder... ignore it

        //        } else {   // this is a dataset...
        //            Dataset ds = GetByUniqueKey(s3o.Key);
        //            dsList.Add(ds);
        //        }
        //    }
        //    return dsList.AsQueryable();
        //}

        ///// <summary>
        ///// (private) recursively convert a list of S3Objects to a heirarchical DatasetFolder (with populated subfolders)
        ///// </summary>
        ///// <param name="s3oList"></param>
        ///// <param name="parentFolder"></param>
        //private void S3ObjectToDatasetFolder(List<S3Object> s3oList, DatasetFolder parentFolder)
        //{
        //    // get all top-level folders under the current folder...
        //    // these will end with a backslash "/" and, if we remove the parentFolder from consideration,
        //    // they should only have one "/" so first and last index thereof should be the same...
        //    List<S3Object> topLevelSubFolders;
        //    if (null == parentFolder.Key || parentFolder.Key.Length == 0) {
        //        topLevelSubFolders = s3oList.Where(s => s.Key.EndsWith("/") && s.Key.IndexOf("/") == s.Key.LastIndexOf("/")).ToList();
        //    } else {
        //        topLevelSubFolders = s3oList.Where(s => s.Key != parentFolder.Key && s.Key.EndsWith("/") &&
        //                                                s.Key.Replace(parentFolder.Key, " ").IndexOf("/") ==
        //                                                s.Key.Replace(parentFolder.Key, " ").LastIndexOf("/")).ToList();
        //    }
        //    foreach (S3Object tlsf in topLevelSubFolders) {

        //        // add this folder to parent
        //        string nameStub = tlsf.Key.TrimEnd("/".ToCharArray());
        //        DatasetFolder dsf = new DatasetFolder(
        //            tlsf.BucketName, 
        //            tlsf.Key,
        //            tlsf.ETag,
        //            nameStub.Contains("/") ?
        //                nameStub.Substring(nameStub.LastIndexOf("/")).Replace("/", "") : nameStub,
        //            parentFolder);

        //        //parentFolder.SubFolders.Add(dsf);
        //        // process this folder...
        //        List<S3Object> children = s3oList.Where(s => s.Key.StartsWith(tlsf.Key)).ToList();
        //        S3ObjectToDatasetFolder(children, dsf);
        //    }

        //    // get all non-folder children at specifically this level...
        //    // these should not end in "/", and should not contain any intermediate "/" either
        //    List<S3Object> childrenHere;
        //    if (null == parentFolder.Key || parentFolder.Key.Length == 0) {
        //        childrenHere = s3oList.Where(s => (!s.Key.EndsWith("/")) &&
        //                                          (!s.Key.Contains("/"))).ToList();
        //    } else {
        //        childrenHere = s3oList.Where(s => (!s.Key.EndsWith("/")) &&
        //                                          (!s.Key.Replace(parentFolder.Key, "").Contains("/"))).ToList();
        //    }

        //    foreach (S3Object c in childrenHere) {
        //        String categoryName = c.Key.Substring(0, c.Key.IndexOf("/"));
        //        Dictionary<string, string> md = GetObjectMetadata(c);
        //        Dataset ds = new Dataset(
        //            c.ETag.Replace("\"",""),
        //            c.Key,
        //            c.BucketName, 
        //            c.Key.Substring(c.Key.LastIndexOf("/")).Replace("/",""),
        //            c.LastModified,
        //            null,
        //            md.ContainsKey("desc") ? md["desc"] : null,
        //            null, 
        //            categoryName,
        //            md);
        //        parentFolder.Datasets.Add(ds);
        //    }
        //}

        //private Dictionary<string, string> GetObjectMetadata(S3Object s3Obj)
        //{
        //    GetObjectMetadataRequest req = new GetObjectMetadataRequest();
        //    req.BucketName = s3Obj.BucketName;
        //    req.Key = s3Obj.Key;
        //    GetObjectMetadataResponse rsp = S3Client.GetObjectMetadata(req);
        //    Dictionary<string, string> md = new Dictionary<string, string>();
        //    md.Add("S3 - ETag", s3Obj.ETag);
        //    md.Add("S3 - Size", s3Obj.Size.ToString());
        //    md.Add("S3 - Last Modified", s3Obj.LastModified.ToShortDateString() + " " + s3Obj.LastModified.ToShortTimeString());
        //    foreach (string mdKey in rsp.Metadata.Keys)
        //    {
        //        md.Add(mdKey.Replace("x-amz-meta-", ""), rsp.Metadata[mdKey]);
        //    }
        //    return md;
        //}
    }
}

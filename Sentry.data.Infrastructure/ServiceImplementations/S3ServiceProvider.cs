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
using System.IO;
using Amazon.S3.Transfer;
using Sentry.Common.Logging;
using Amazon.S3.IO;

namespace Sentry.data.Infrastructure
{
    public sealed class S3ServiceProvider : IDatasetService
    {
        private static S3ServiceProvider instance = null;
        private static readonly object padlock = new object();

        public S3ServiceProvider() { }

        public static S3ServiceProvider Instance
        {
            get
            {
                lock (padlock)
                {
                    if(instance == null)
                    {
                        instance = new S3ServiceProvider();
                    }
                    return instance;
                }
            }
        }

        private static Amazon.S3.IAmazonS3 _s3client = null;

        public event EventHandler<TransferProgressEventArgs> OnTransferProgressEvent;
        
        private Amazon.S3.IAmazonS3 S3Client
        {
            get
            {
                if (null == _s3client)
                {
                    // instantiate a new shared client
                    AWSConfigsS3.UseSignatureVersion4 = true;
                    AmazonS3Config s3config = new AmazonS3Config();
                    // s3config.RegionEndpoint = RegionEndpoint.GetBySystemName("us-east-1");
                    s3config.RegionEndpoint = RegionEndpoint.GetBySystemName(Configuration.Config.GetSetting("AWSRegion"));
                    //s3config.UseHttp = true;
                    s3config.ProxyHost = Configuration.Config.GetHostSetting("SentryS3ProxyHost");
                    s3config.ProxyPort = int.Parse(Configuration.Config.GetSetting("SentryS3ProxyPort"));
                    s3config.ProxyCredentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    string awsAccessKey = Configuration.Config.GetHostSetting("AWSAccessKey");
                    string awsSecretKey = Configuration.Config.GetHostSetting("AWSSecretKey");
                    _s3client = new AmazonS3Client(awsAccessKey, awsSecretKey, s3config);
                }
                return _s3client;
            }
        }

        /// <summary>
        /// Retrieves a presigned URL for the S3 Object. versionId is optional, if not supplied
        /// default is null and will return latest version of S3 key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public string GetDatasetDownloadURL(string key, string versionId = null)
        {
            GetPreSignedUrlRequest req = new GetPreSignedUrlRequest()
            {
                BucketName = Configuration.Config.GetHostSetting("AWSRootBucket"),
                Key = key,
                VersionId = versionId,
                Expires = DateTime.Now.AddMinutes(2)
            };
            //setting content-disposition to attachment vs. inline (into browser) to force "save as" dialog box for all doc types.
            req.ResponseHeaderOverrides.ContentDisposition = "attachment";
            string url = S3Client.GetPreSignedURL(req);
            return url;
        }

        /// <summary>
        /// Retrieves a presigned URL for the S3 Object.
        /// Encoding can be supplied optionally.  If not, ContentDisposition set to attachement
        /// default is null and will return latest version of S3 key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ecode"></param>
        /// <returns></returns>
        public string GetUserGuideDownloadURL(string key, string ecode = null)
        {
            GetPreSignedUrlRequest req = new GetPreSignedUrlRequest()
            {
                BucketName = Configuration.Config.GetHostSetting("AWSRootBucket"),
                Key = key,
                VersionId = null,
                Expires = DateTime.Now.AddMinutes(2)
            };
            //setting content-disposition to attachment vs. inline (into browser) to force "save as" dialog box for all doc types.
            
            if (ecode == null)
            {
                req.ResponseHeaderOverrides.ContentDisposition = "attachment";
            }
            else
            {
                req.ResponseHeaderOverrides.ContentEncoding = ecode;
            }
            
            string url = S3Client.GetPreSignedURL(req);
            return url;
        }

        /// <summary>
        /// Upload a dataset to S3, pulling directly from the given source file path.  Files size less than
        /// 5MB will use PutObject, larger than 5MB will utilize MultiPartUpload.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="dataSet"></param>
        public string UploadDataFile(string sourceFilePath, string targetKey)
        {
            string versionId = null;

            System.IO.FileInfo fInfo = new System.IO.FileInfo(sourceFilePath);
            
            if (fInfo.Length > 5 * (long)Math.Pow(2, 20))
            {
                versionId = MultiPartUpload(sourceFilePath, targetKey);
            }
            else
            {
                versionId = PutObject(sourceFilePath, targetKey);
            }

            return versionId;
        }

        /// <summary>
        /// Upload file to S3 use a Stream input.  Only utilizes PutObject and limited to 5GB in size.
        /// </summary>
        /// <param name="inputstream"></param>
        /// <param name="targetKey"></param>
        /// <returns></returns>
        public string UploadDataFile(Stream inputstream, string targetKey)
        {
            string versionId = null;

                versionId = PutObject(inputstream, targetKey);

            return versionId;
        }

        public void TransferUtlityUploadStream(string folder, string fileName, Stream stream)
        {
            Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: Started S3 TransferUtility Setup");
            try
            {
                Amazon.S3.Transfer.TransferUtility s3tu = new Amazon.S3.Transfer.TransferUtility(S3Client);
                Amazon.S3.Transfer.TransferUtilityUploadRequest s3tuReq = new Amazon.S3.Transfer.TransferUtilityUploadRequest();
                //Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - Set AWS BucketName: " + Configuration.Config.GetSetting("AWSRootBucket"));
                s3tuReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                //Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - InputStream");
                s3tuReq.InputStream = stream;
                //Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - Set S3Key: " + category + "/" + dsfi);
                s3tuReq.Key = folder + fileName;
                s3tuReq.UploadProgressEvent += new EventHandler<UploadProgressArgs>(a_TransferProgressEvent);
                s3tuReq.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
                s3tuReq.AutoCloseStream = true;
                //Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: Starting Upload " + s3tuReq.Key);
                s3tu.Upload(s3tuReq);
            }
            catch (AmazonS3Exception e)
            {
                
                throw new Exception("Error attempting to transfer fileto S3.", e);
            }

        }

        public void TransferUtlityUploadStream(string key, Stream stream)
        {
            Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: Started S3 TransferUtility Setup");
            try
            {
                Amazon.S3.Transfer.TransferUtility s3tu = new Amazon.S3.Transfer.TransferUtility(S3Client);
                Amazon.S3.Transfer.TransferUtilityUploadRequest s3tuReq = new Amazon.S3.Transfer.TransferUtilityUploadRequest();
                //Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - Set AWS BucketName: " + Configuration.Config.GetSetting("AWSRootBucket"));
                s3tuReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                //Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - InputStream");
                s3tuReq.InputStream = stream;
                //Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: TransferUtility - Set S3Key: " + category + "/" + dsfi);
                s3tuReq.Key = key;
                s3tuReq.UploadProgressEvent += new EventHandler<UploadProgressArgs>(a_TransferProgressEvent);
                s3tuReq.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
                s3tuReq.AutoCloseStream = true;
                //Sentry.Common.Logging.Logger.Debug("HttpPost <Upload>: Starting Upload " + s3tuReq.Key);
                s3tu.Upload(s3tuReq);
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception("Error attempting to transfer fileto S3.", e);
            }

        }

        public void TransferUtilityDownload(string baseTargetPath, string folder, string filename, string s3Key)
        {
            try
            {
                Sentry.Common.Logging.Logger.Debug("Started S3 TransferUtility Setup for Download");
                Amazon.S3.Transfer.TransferUtility s3tu = new Amazon.S3.Transfer.TransferUtility(S3Client);
                Amazon.S3.Transfer.TransferUtilityDownloadRequest s3tuDwnldReq = new Amazon.S3.Transfer.TransferUtilityDownloadRequest();
                Sentry.Common.Logging.Logger.Debug("TransferUtility - Set AWS BucketName: " + Configuration.Config.GetHostSetting("AWSRootBucket"));
                s3tuDwnldReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                Sentry.Common.Logging.Logger.Debug("TransferUtility - Set FilePath: " + baseTargetPath + folder + @"\" + filename);
                s3tuDwnldReq.FilePath = baseTargetPath + folder + @"\" + filename;

                s3tuDwnldReq.Key = s3Key;

                s3tuDwnldReq.WriteObjectProgressEvent += new EventHandler<WriteObjectProgressArgs>(a_TransferProgressEvent);
                //s3tuDwnldReq.WriteObjectProgressEvent += new EventHandler<WriteObjectProgressArgs>(downloadRequest_DownloadPartProgressEvent);

                s3tu.Download(s3tuDwnldReq);
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception("Error attempting to download file from S3.", e);
            }
        }

        private void a_TransferProgressEvent(object sender, WriteObjectProgressArgs e)
        {
            //OnTransferProgressEvent(this, new TransferProgressEventArgs(e.FilePath, e.PercentDone));
            EventHandler<TransferProgressEventArgs> handler = OnTransferProgressEvent;
            if (handler != null)
            {
                handler(this, new TransferProgressEventArgs(Path.GetFileName(e.FilePath), e.PercentDone, "Downloading"));
            }
            
            //if (handler != null)
            //{
            //    handler(this, e);
            //}
        }

        private void a_TransferProgressEvent(object sender, UploadProgressArgs e)
        {
            //TransferProgressEventArgs args = new TransferProgressEventArgs(e.FilePath, e.PercentDone);
            //OnTransferProgressEvent(args);
            OnTransferProgressEvent(this, new TransferProgressEventArgs(Path.GetFileName(e.FilePath), e.PercentDone, "Uploading"));
            //EventHandler<WriteObjectProgressArgs> handler = OnTransferProgressEvent;
            //if (handler != null)
            //{
            //    handler(this, e);
            //}
        }

        #region MultiPartUpload

        public string MultiPartUpload(string sourceFilePath, string targetKey)
        {
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            string versionId = null;
            
            string uploadId = StartUpload(targetKey);

            long contentLength = new FileInfo(sourceFilePath).Length;
            long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB

            try
            {
                long filePositiion = 0;
                for (int i = 1; filePositiion < contentLength; i++)
                {
                    //Adding responses to list as returned ETags are needed to close Multipart upload
                    UploadPartResponse resp = UploadPart(targetKey, sourceFilePath, filePositiion, partSize, i, uploadId);

                    Sentry.Common.Logging.Logger.Debug($"UploadID: {uploadId}: Processed part #{i} (source file position: {filePositiion}), and recieved response status {resp.HttpStatusCode} with ETag ({resp.ETag})");

                    uploadResponses.Add(resp);

                    filePositiion += partSize;
                }

                //Complete successful Multipart Upload so we do not continue to get chared for upload storage
                versionId = StopUpload(targetKey, uploadId, uploadResponses);

            }
            catch (Exception ex)
            {
                Sentry.Common.Logging.Logger.Error("Error Processing MultiPartUpload", ex);
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = Configuration.Config.GetHostSetting("AWSRootBucket"),
                    Key = targetKey,
                    UploadId = uploadId
                };

                Sentry.Common.Logging.Logger.Debug($"Aborting MultipartUpload UploadID: {uploadId}");
                S3Client.AbortMultipartUpload(abortMPURequest);

                throw new Exception("Error attempting to upload dataset to S3: " + ex.Message);
            }

            return versionId;
        }

        /// <summary>
        /// Initiate Multipart Upload Request and return UploadID used for each part upload
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <returns></returns>
        public string StartUpload(string uniqueKey)
        {
            InitiateMultipartUploadRequest mReq = new InitiateMultipartUploadRequest();
            mReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            mReq.Key = uniqueKey;
            mReq.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
            InitiateMultipartUploadResponse mRsp = S3Client.InitiateMultipartUpload(mReq);

            Sentry.Common.Logging.Logger.Debug($"Initiated MultipartUpload UploadID: {mRsp.UploadId}");

            return mRsp.UploadId;
        }

        private UploadPartResponse UploadPart(string uniqueKey, string sourceFilePath, long filePosition, long partSize, int partNumber, string uploadId)
        {
            UploadPartRequest uReq = new UploadPartRequest();
            uReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            uReq.Key = uniqueKey;
            uReq.FilePath = sourceFilePath;
            uReq.FilePosition = filePosition;
            uReq.PartSize = partSize;
            uReq.PartNumber = partNumber;
            uReq.UploadId = uploadId;            
            uReq.StreamTransferProgress += new EventHandler<Amazon.Runtime.StreamTransferProgressArgs>(UploadProgressEventCallbackHandler);
            UploadPartResponse uRsp = S3Client.UploadPart(uReq);
            if (uRsp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error attempting to upload dataset to S3: " + uRsp.HttpStatusCode);
            }
            return uRsp;
        }

        /// <summary>
        /// Complete Multipart upload and return version ID
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        private string StopUpload(string uniqueKey, string uploadId, List<UploadPartResponse> responses)
        {
            CompleteMultipartUploadRequest cReq = new CompleteMultipartUploadRequest();
            cReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            cReq.Key = uniqueKey;
            cReq.UploadId = uploadId;
            cReq.AddPartETags(responses);


            CompleteMultipartUploadResponse mRsp = S3Client.CompleteMultipartUpload(cReq);

            Sentry.Common.Logging.Logger.Debug($"Completed MultipartUpload UploadID: {uploadId}, with response status {mRsp.HttpStatusCode}");

            //return mRsp.ETag;
            return mRsp.VersionId;
        }

        private static void UploadProgressEventCallbackHandler(object sender, Amazon.Runtime.StreamTransferProgressArgs e)
        {
            int percent = Convert.ToInt32(Convert.ToDouble(e.PercentDone));

            //Only log every 10%
            if (percent % 10 == 0)
            {
                Sentry.Common.Logging.Logger.Debug($"Percent Done: {percent}, TransferredBytes: {e.TransferredBytes}, TotalBytes: {e.TotalBytes}");
            }
        }

        #endregion

        #region MultiPartUpload - Copy

        public string MultiPartCopy(List<Tuple<string, string>> sourceKeys, string targetKey)
        {
            throw new NotImplementedException();
            //List<CopyPartResponse> copyPartResponses = new List<CopyPartResponse>();
            //string versionId = null;
            //int partnum = 1;

            //string uploadId = StartUpload(targetKey);

            //for (int i = 0; i < sourceKeys.Count(); i++)
            //{
            //    Tuple<string, string> obj = sourceKeys[i];

            //    CopyPartResponse resp = CopyPart(targetKey, partnum, obj.Item1, obj.Item2, uploadId);

            //    copyPartResponses.Add(resp);

            //    partnum++;
            //}

            //versionId = StopUpload(targetKey, uploadId, copyPartResponses);



            ////long contentLength = new FileInfo(sourceFilePath).Length;
            ////long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB

            //return versionId;
        }

        public CopyPartResponse CopyPart(string dest_Key, int partnum, string source_key, string source_versionId, string uploadId)
        {
            throw new NotImplementedException();
            //CopyPartRequest req = new CopyPartRequest();
            //req.DestinationBucket = Configuration.Config.GetHostSetting("AWSRootBucket");
            //req.DestinationKey = dest_Key;
            //req.PartNumber = partnum;
            //req.SourceBucket = Configuration.Config.GetHostSetting("AWSRootBucket");
            //req.SourceKey = source_key;
            //req.SourceVersionId = source_versionId;
            //req.UploadId = uploadId;

            //CopyPartResponse response = S3Client.CopyPart(req);

            //return response;
        }

        private string StopUpload(string uniqueKey, string uploadId, List<CopyPartResponse> responses)
        {
            throw new NotImplementedException();
            //CompleteMultipartUploadRequest cReq = new CompleteMultipartUploadRequest();
            //cReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            //cReq.Key = uniqueKey;
            //cReq.UploadId = uploadId;
            //cReq.AddPartETags(responses);

            //CompleteMultipartUploadResponse mRsp = S3Client.CompleteMultipartUpload(cReq);
           
            //Sentry.Common.Logging.Logger.Debug($"Completed MultipartUpload UploadID: {uploadId}, with response status {mRsp.HttpStatusCode}");

            ////return mRsp.ETag;
            //return mRsp.VersionId;
        }

        #endregion
                 
        #region PutObject

        private string PutObject(Stream filestream, string targetKey)
        {
            string versionId = null;
            try
            {
                PutObjectRequest poReq = new PutObjectRequest();
                poReq.InputStream = filestream;
                poReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                poReq.Key = targetKey;
                poReq.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
                poReq.AutoCloseStream = true;                

                Sentry.Common.Logging.Logger.Debug($"Initialized PutObject Request: Bucket:{poReq.BucketName}, File:{poReq.FilePath}, Key:{targetKey}");

                PutObjectResponse poRsp = S3Client.PutObject(poReq);

                Sentry.Common.Logging.Logger.Debug($"Completed PutObject Request: Key: {targetKey}, Version_ID:{poRsp.VersionId}, ETag:{poRsp.ETag}, Lenght(bytes):{poRsp.ContentLength}");

                versionId = poRsp.VersionId;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Error attempting to upload dataset to S3: Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error attempting to upload dataset to S3: " + amazonS3Exception.Message);
                }
            }
            return versionId;
        }

        private string PutObject(string sourceFilePath, string targetKey)
        {
            string versionId = null;
            try
            {
                PutObjectRequest poReq = new PutObjectRequest();
                poReq.FilePath = sourceFilePath;
                poReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                poReq.Key = targetKey;
                poReq.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;

                Sentry.Common.Logging.Logger.Debug($"Initialized PutObject Request: Bucket:{poReq.BucketName}, File:{poReq.FilePath}, Key:{targetKey}");

                PutObjectResponse poRsp = S3Client.PutObject(poReq);

                Sentry.Common.Logging.Logger.Debug($"Completed PutObject Request: Key: {targetKey}, Version_ID:{poRsp.VersionId}, ETag:{poRsp.ETag}, Lenght(bytes):{poRsp.ContentLength}");

                versionId = poRsp.VersionId;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Error attempting to upload dataset to S3: Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error attempting to upload dataset to S3: " + amazonS3Exception.Message);
                }
            }
            return versionId;
        }

        #endregion

        #region Delete Object
        /// <summary>
        /// Place a s3 Delete marker on the S3 Object.  This prevents any GetObject
        /// command (with null version ID) to retrieve objects.  You may still retrieve
        /// specific version if you pass version ID with GetObject command.
        /// http://docs.aws.amazon.com/AmazonS3/latest/dev/ObjectVersioning.html
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ObjectKeyVersion MarkDeleted(string key)
        {
            DeleteObjectRequest doReq = new DeleteObjectRequest();
            doReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            doReq.Key = key;
            DeleteObjectResponse doRsp = S3Client.DeleteObject(doReq);

            //return the delete marker version ID with original key
            ObjectKeyVersion returnobj = new ObjectKeyVersion();
            returnobj.key = key;
            returnobj.versionId = doRsp.VersionId;

            return returnobj;
        }

        /// <summary>
        /// Permanently deletes S3 object.  Since our
        /// </summary>
        /// <param name="keyVersion"></param>
        /// <returns></returns>
        public void DeleteS3Key(ObjectKeyVersion keyVersion)
        {
            try
            {
                DeleteObjectRequest doReq = new DeleteObjectRequest();
                doReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                doReq.Key = keyVersion.key;
                DeleteObjectResponse doRsp = S3Client.DeleteObject(doReq);

                //return the delete marker version ID
                ObjectKeyVersion returnobj = new ObjectKeyVersion();
                returnobj.key = keyVersion.key;
                returnobj.versionId = doRsp.VersionId;

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")
                    ||
                    amazonS3Exception.ErrorCode.Equals("Forbidden")))
                {
                    throw new Exception($"Failed DeleteS3Key - Check the provided AWS Credentials ({amazonS3Exception.Message})");
                }
                else
                {
                    throw new Exception("Error attempting to delete S3 key: " + amazonS3Exception.InnerException);
                }
            }
        }

        public void DeleteMultipleS3keys(List<ObjectKeyVersion> keyversionids)
        {
            List<KeyVersion> objects = new List<KeyVersion>();
            List<ObjectKeyVersion> deletedObjects = new List<ObjectKeyVersion>();

            foreach (ObjectKeyVersion obj in keyversionids)
            {
                objects.Add(ToKeyVersion(obj));
            }

            try
            {
                DeleteObjectsRequest dosReq = new DeleteObjectsRequest();
                dosReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                dosReq.Objects = objects;
                DeleteObjectsResponse dosRsp = S3Client.DeleteObjects(dosReq);
                Logger.Info($"No. of objects successfully deleted = {dosRsp.DeletedObjects.Count()}");

                foreach (DeletedObject dobj in dosRsp.DeletedObjects)
                {
                    ObjectKeyVersion newItem = new ObjectKeyVersion();
                    newItem.key = dobj.Key;
                    newItem.versionId = dobj.VersionId;
                    deletedObjects.Add(newItem);
                }

            }
            catch (DeleteObjectsException e)
            {
                DeleteObjectsResponse dosRsp = e.Response;
                StringBuilder sb = new StringBuilder();
                foreach (DeleteError error in dosRsp.DeleteErrors)
                {
                    sb.Append($"Object Key: {error.Key} Object VersionID: {error.VersionId} Code: {error.Code} Message:{error.Message}");
                }
                Logger.Error($"Successfully deleted = {dosRsp.DeletedObjects.Count()} : Failed to Delete = {dosRsp.DeleteErrors.Count()}", new Exception(sb.ToString()));
                throw new Exception($"Failed DeleteMultipleS3keys: Failed to Delete {dosRsp.DeleteErrors.Count()} keys", new Exception(sb.ToString()));
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")
                    ||
                    amazonS3Exception.ErrorCode.Equals("Forbidden")))
                {
                    throw new Exception($"Failed DeleteS3Key - Check the provided AWS Credentials ({amazonS3Exception.Message})");
                }
                else
                {
                    throw new Exception("Error attempting to delete S3 keys: " + amazonS3Exception.InnerException);
                }
            }
        }
        #endregion
        
        /// <summary>
        /// Get list of datasets currently on S3, within the given parentDir
        /// </summary>
        /// <param name="parentDir"></param>
        /// <param name="includeSubDirectories"></param>
        /// <returns></returns>
        public IDictionary<string, string> GetDatasetList(string parentDir = "sentry-dataset-management", bool includeSubDirectories = true)
        {
            // this will include folders in addition to data sets...
            if (parentDir == null)
            {
                parentDir = Configuration.Config.GetHostSetting("AWSRootBucket");
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

        public Dictionary<string,string> GetObjectMetadata(string key, string versionId)
        {
            GetObjectMetadataRequest req = new GetObjectMetadataRequest();
            GetObjectMetadataResponse resp = null;

            req.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            req.Key = key;
            req.VersionId = versionId;

            try
            {
                resp = S3Client.GetObjectMetadata(req);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")
                    ||
                    amazonS3Exception.ErrorCode.Equals("Forbidden")))
                {
                    throw new Exception($"Failed GetObjectMetadata - Check the provided AWS Credentials ({amazonS3Exception.Message})");                    
                }
                else
                {
                    throw new Exception($"Failed GetObjectMetadata - {amazonS3Exception.Message}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ConvertObjectMetadataResponse(resp);
        }
        
        //public string GetObject(string key, string versionId)
        //{
        //    throw new NotImplementedException();

        //    //GetObjectRequest req = new GetObjectRequest();
        //    //string contents = null;

        //    //req.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
        //    //req.Key = key;
        //    //req.VersionId = versionId;

        //    //using (GetObjectResponse response = S3Client.GetObject(req))
        //    //{
        //    //    using (StreamReader reader = new StreamReader(response.ResponseStream))
        //    //    {
        //    //        contents = reader.ReadToEnd();
        //    //    }
        //    //}

        //    //return contents;
        //}

        /// <summary>
        /// Returns an S3 object as a Stream.  By default versionID is null, therefore, will
        /// return the latest verison of the object (if versions exist).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public Stream GetObject(string key, string versionId = null)
        {

            GetObjectRequest req = new GetObjectRequest();
            GetObjectResponse response = new GetObjectResponse();

            req.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            req.Key = key;
            req.VersionId = versionId;

            
            try
            {
                response = S3Client.GetObject(req);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Failed GetObject - Check the provided AWS Credentials");
                }
                else
                {
                    throw new Exception($"Failed GetObject - {amazonS3Exception.Message}");
                }
            }
            return response.ResponseStream;            
        }

        public List<string> FindObject(string keyPrefix)
        {
            var request = new ListObjectsRequest();

            request.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            request.Prefix = keyPrefix;

            ListObjectsResponse response = S3Client.ListObjects(request);

            return response.S3Objects.Select(x => x.Key).ToList();

        }

        /// <summary>
        /// Lists all objects
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IList<string> ListObjects(string bucket, string prefix)
        {
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException(bucket,"Parameter is required");
            }
            else if(string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentNullException(prefix, "Parameter is required");
            }

            // List all objects
            ListObjectsRequest listRequest = new ListObjectsRequest
            {
                BucketName = bucket,
                Prefix = prefix
            };

            List<string> objectlist = new List<string>();

            ListObjectsResponse listResponse;
            do
            {
                // Get a list of objects
                listResponse = S3Client.ListObjects(listRequest);
                foreach (S3Object obj in listResponse.S3Objects)
                {
                    //Remove prefix object (folder)
                    if (obj.Key != prefix)
                    {
                        objectlist.Add(obj.Key);
                    }                    
                }
                // Set the marker property
                listRequest.Marker = listResponse.NextMarker;
            } while (listResponse.IsTruncated);

            return objectlist;
        }

        public string CopyObject(string srcBucket, string srcKey, string destBucket, string destKey)
        {
            //S3FileInfo source = new S3FileInfo(S3Client, srcBucket, srcKey);
            //S3FileInfo target = new S3FileInfo(S3Client, destBucket, destKey);
            //source.CopyTo(target, true);

            Dictionary<string, string> resp = null;

            CopyObjectRequest request = new CopyObjectRequest
            {
                SourceBucket = srcBucket,
                SourceKey = srcKey,
                DestinationBucket = destBucket,
                DestinationKey = destKey,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256                
            };

            CopyObjectResponse response = S3Client.CopyObject(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                resp = GetObjectMetadata(destKey, null);
            }
            return (resp != null) ? Convert.ToString(resp["VersionId"]) : null;
        }

        #region Helpers

        private KeyVersion ToKeyVersion (ObjectKeyVersion input)
        {
            KeyVersion key = null;
            key.Key = input.key;
            key.VersionId = input.versionId;
            return key;
        }
        private List<List<string>> GetPartList(List<string> sourceKeys)
        {
            throw new NotImplementedException();

            foreach (string key in sourceKeys)
            {
                int size = GetObjectSize(key);
            }

        }
        private int GetObjectSize(string key)
        {
            GetObjectMetadataRequest headReq = new GetObjectMetadataRequest();



            throw new NotImplementedException();
        }
        private Dictionary<string, string> ConvertObjectMetadataResponse(GetObjectMetadataResponse resp)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            output.Add("ContentLength", resp.ContentLength.ToString());
            output.Add("ETag", resp.ETag);
            output.Add("VersionId", resp.VersionId);

            return output;

        }
        
        #endregion
        
    }
}

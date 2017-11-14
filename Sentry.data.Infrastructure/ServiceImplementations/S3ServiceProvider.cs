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

namespace Sentry.data.Infrastructure
{
    public class S3ServiceProvider : NHReadableStatelessDomainContext, IDatasetService
    {
        public S3ServiceProvider(IStatelessSession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<S3ServiceProvider>();

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
                    // TODO: move this all to the config(s)...
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
        /// Get a presigned URL for the dataset in question.
        /// This can be loaded straight into a client browser in order to support faster downloads of large files.
        /// </summary>
        public string GetDatasetDownloadURL(string uniqueKey)
        {
            //var bucketName = Configuration.Config.GetSetting("AWSRootBucket");
            //return S3Client.GetObjectStream(bucketName, uniqueKey, null);

            GetPreSignedUrlRequest req = new GetPreSignedUrlRequest()
            {
                BucketName = Configuration.Config.GetHostSetting("AWSRootBucket"),
                Key = uniqueKey,
                Expires = DateTime.Now.AddMinutes(2)
            };
            //setting content-disposition to attachment vs. inline (into browser) to force "save as" dialog box for all doc types.
            req.ResponseHeaderOverrides.ContentDisposition = "attachment";
            string url = S3Client.GetPreSignedURL(req);
            return url;
        }

        public string GetDatasetDownloadURL(string key, string versionId)
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

        public string GetObjectPreview(string key)
        {
            string contents = null;

            GetObjectRequest getReq = new GetObjectRequest();
            getReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            getReq.Key = key;
            

            GetObjectResponse getRsp = S3Client.GetObject(getReq);
            if (getRsp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error attempting to perform get object on S3: " + getRsp.HttpStatusCode);
            }

            using (Stream stream = getRsp.ResponseStream)
            {
                long length = stream.Length;
                byte[] bytes = new byte[length];
                stream.Read(bytes,0,(int)length);
                contents = Encoding.UTF8.GetString(bytes);
            }

            return contents;

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

        protected virtual void a_TransferProgressEvent(object sender, WriteObjectProgressArgs e)
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

        protected virtual void a_TransferProgressEvent(object sender, UploadProgressArgs e)
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
        private string StartUpload(string uniqueKey)
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

        #region PutObject

        private string PutObject(string sourceFilePath, string targetKey)
        {
            string versionId = null;
            try
            {
                PutObjectRequest poReq = new PutObjectRequest();
                poReq.FilePath = sourceFilePath;
                poReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                poReq.Key = targetKey;
                System.IO.FileInfo fInfo = new System.IO.FileInfo(sourceFilePath);
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
        
        /// <summary>
        /// Delete a dataset from S3
        /// </summary>
        /// <param name="uniqueKey"></param>
        public void DeleteDataset(string uniqueKey)
        {
            DeleteObjectRequest doReq = new DeleteObjectRequest();
            doReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
            doReq.Key = uniqueKey;
            DeleteObjectResponse doRsp = S3Client.DeleteObject(doReq);
            if (doRsp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Error attempting to delete dataset from S3: " + doRsp.HttpStatusCode);
            }
        }

        public void DeleteS3key(string key)
        {
            //IAmazonS3 client = null;
            //using(client = S3Client)
            //{
                try
                {
                    DeleteObjectRequest doReq = new DeleteObjectRequest();
                    doReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                    doReq.Key = key;
                    DeleteObjectResponse doRsp = S3Client.DeleteObject(doReq);
                }
                catch (AmazonS3Exception s3Exception)
                {
                    throw new Exception("Error attempting to delete S3 key: " + s3Exception.InnerException);
                }
            //}
        }        

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
                
    }
}

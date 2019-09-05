using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Sentry.data.Core;
using System.IO;
using Sentry.Common.Logging;
using Amazon.S3.IO;

namespace Sentry.data.Infrastructure
{
    public class S3ServiceProvider : IS3ServiceProvider
    {
        private static S3ServiceProvider instance = null;
        private static readonly object padlock = new object();
        private static string versionId;

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
        
        private static Amazon.S3.IAmazonS3 S3Client
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
        public string GetDatasetDownloadURL(string key, string versionId = null, string fileName = null)
        {
            GetPreSignedUrlRequest req = new GetPreSignedUrlRequest()
            {
                BucketName = Configuration.Config.GetHostSetting("AWSRootBucket"),
                Key = key,
                VersionId = versionId,
                Expires = DateTime.Now.AddMinutes(2)
            };
            //setting content-disposition to attachment vs. inline (into browser) to force "save as" dialog box for all doc types.
            req.ResponseHeaderOverrides.ContentDisposition = fileName != null ? "attachment; filename = " + fileName : "attachment";
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
            System.IO.FileInfo fInfo = new System.IO.FileInfo(sourceFilePath);

            return fInfo.Length > 5 * (long)Math.Pow(2, 20) ? MultiPartUpload(sourceFilePath, targetKey) : PutObject(sourceFilePath, targetKey);
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
                s3tuReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");
                s3tuReq.InputStream = stream;
                s3tuReq.Key = folder + fileName;
                s3tuReq.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
                s3tuReq.AutoCloseStream = true;
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

                s3tuReq.BucketName = Configuration.Config.GetHostSetting("AWSRootBucket");

                s3tuReq.InputStream = stream;

                s3tuReq.Key = key;
                s3tuReq.ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256;
                s3tuReq.AutoCloseStream = true;

                s3tu.Upload(s3tuReq);
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception("Error attempting to transfer fileto S3.", e);
            }

        }

        public void TransferUtilityDownload(string baseTargetPath, string folder, string filename, string s3Key, string versionId = null)
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
                s3tuDwnldReq.VersionId = versionId;
                
                s3tu.Download(s3tuDwnldReq);
            }
            catch (AmazonS3Exception e)
            {
                throw new Exception("Error attempting to download file from S3.", e);
            }
        }


        #region MultiPartUpload

        
        private bool IsPartSizeToSmall(long incomingLength, long partSize, long partLimit)
        {
            //Number of parts need to be less that 95% of S3 multipart limit ("buffer" set by us)
            return ((incomingLength / partSize) > (partLimit * .95));            
        }

        public string MultiPartUpload(string sourceFilePath, string targetKey)
        {
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            string versionId = null;
            
            string uploadId = StartUpload(targetKey);

            long contentLength = new FileInfo(sourceFilePath).Length;

                        
            long partSizeSeed = 5;
            long partSize = partSizeSeed * (long)Math.Pow(2, 20); // 5 MB
            long partSizeUpperLimit = 5 * (long)Math.Pow(2, 30); // 5GB
            long partLimit = 10000;

            //Determine PartSize that will not exceed 10,000 parts (s3 multipart upload limit).  In addition, 
            //  leaving a buffer of 5% of 10,000.  Process will initially start with 5MB parts, and incremetally 
            //  increase by 1MB until buffer >= 5% part limit.
            while (IsPartSizeToSmall(contentLength, partSize, partLimit))
            {
                partSizeSeed++;
                partSize = partSizeSeed * (long)Math.Pow(2, 20);
            }

            //Check part size upper limit
            if (partSize > partSizeUpperLimit)
            {
                throw new NotSupportedException("Multi-Upload part size exceeds 5GB limit");
            }

            Logger.Info($"Calculated part size - size(bytes):{partSize}");

            try
            {
                long filePosition = 0;
                int partnumber = 1;
                while (filePosition < contentLength)
                {
                    //Adding responses to list as returned ETags are needed to close Multipart upload
                    UploadPartResponse resp = UploadPart(targetKey, sourceFilePath, filePosition, partSize, partnumber, uploadId);

                    Sentry.Common.Logging.Logger.Debug($"UploadID: {uploadId}: Processed part #{partnumber} (source file position: {filePosition}), and recieved response status {resp.HttpStatusCode} with ETag ({resp.ETag})");

                    uploadResponses.Add(resp);

                    filePosition += partSize;

                    partnumber++;
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

        private async Task MultiPartCopy(string sourceKey, string targetKey)
        {
            List<CopyPartResponse> copyPartResponses = new List<CopyPartResponse>();

            string uploadId = StartUpload(targetKey);

            try
            {
                Dictionary<string, string> metadataresp = GetObjectMetadata(sourceKey);

                long objectSize = Convert.ToInt64(metadataresp["ContentLength"]);

                long partSizeSeed = 5;
                long partSize = partSizeSeed * (long)Math.Pow(2, 20); // 5 MB
                long partSizeUpperLimit = 5 * (long)Math.Pow(2, 30); // 5GB
                long partLimit = 10000;

                //Determine PartSize that will not exceed 10,000 parts (s3 multipart upload limit).  In addition, 
                //  leaving a buffer of 5% of 10,000.  Process will initially start with 5MB parts, and incremetally 
                //  increase by 1MB until buffer >= 5% part limit.
                while (IsPartSizeToSmall(objectSize, partSize, partLimit))
                {
                    partSizeSeed++;
                    partSize = partSizeSeed * (long)Math.Pow(2, 20);
                }

                //Check part size upper limit
                if (partSize > partSizeUpperLimit)
                {
                    throw new NotSupportedException("Multi-Upload part size exceeds 5GB limit");
                }

                Logger.Info($"Calculated part size - size(bytes):{partSize}");

                long bytePosition = 0;
                for (int i = 1; bytePosition < objectSize; i++)
                {
                    CopyPartRequest copyRequest = new CopyPartRequest
                    {
                        DestinationBucket = Configuration.Config.GetHostSetting("AWSRootBucket"),
                        DestinationKey = targetKey,
                        SourceBucket = Configuration.Config.GetHostSetting("AWSRootBucket"),
                        SourceKey = sourceKey,
                        UploadId = uploadId,
                        FirstByte = bytePosition,
                        LastByte = bytePosition + partSize - 1 >= objectSize ? objectSize - 1 : bytePosition + partSize - 1,
                        PartNumber = i
                    };

                    copyPartResponses.Add(await S3Client.CopyPartAsync(copyRequest));

                    CopyPartResponse resp = copyPartResponses.Last();
                    Sentry.Common.Logging.Logger.Debug($"UploadID: {uploadId}: Processed part #{i} (source file position: {bytePosition}), and recieved response status {resp.HttpStatusCode} with ETag ({resp.ETag})");
                    
                    bytePosition += partSize;
                }

                // Complete the copy.
                await StopUpload(targetKey, uploadId, copyPartResponses);
            }

            catch (AmazonS3Exception e)
            {
                Logger.Error($"Error encountered on server. Message:'{e.Message}' when writing an object", e);
                Logger.Info($"Issuing an Abort request - bucket:{Configuration.Config.GetHostSetting("AWSRootBucket")} | key:{targetKey} | uploadid:{uploadId}");
                AbortMultipartUploadRequest abortreq = new AbortMultipartUploadRequest
                {
                    BucketName = Configuration.Config.GetHostSetting("AWSRootBucket"),
                    Key = targetKey,
                    UploadId = uploadId
                };

                AbortMultipartUploadResponse abortresp =  S3Client.AbortMultipartUpload(abortreq);
                Logger.Info($"Abort request was {abortresp.HttpStatusCode.ToString()}");
                throw new AmazonS3Exception(e);
                
            }
            catch (Exception e)
            {
                Logger.Error($"Unknown encountered on server. Message:'{e.Message}' when writing an object", e);
                Logger.Info($"Issuing an Abort request - bucket:{Configuration.Config.GetHostSetting("AWSRootBucket")} | key:{targetKey} | uploadid:{uploadId}");
                AbortMultipartUploadRequest abortreq = new AbortMultipartUploadRequest
                {
                    BucketName = Configuration.Config.GetHostSetting("AWSRootBucket"),
                    Key = targetKey,
                    UploadId = uploadId
                };

                AbortMultipartUploadResponse abortresp = S3Client.AbortMultipartUpload(abortreq);
                Logger.Info($"Abort request was {abortresp.HttpStatusCode.ToString()}");
                throw new AmazonS3Exception(e);
            }
        }

        private static async Task StopUpload(string uniqueKey, string uploadId, List<CopyPartResponse> responses)
        {
            CompleteMultipartUploadRequest cReq = new CompleteMultipartUploadRequest
            { 
                BucketName = Configuration.Config.GetHostSetting("AWSRootBucket"),
                Key = uniqueKey,
                UploadId = uploadId
            };
            cReq.AddPartETags(responses);

            CompleteMultipartUploadResponse mRsp = await S3Client.CompleteMultipartUploadAsync(cReq);

            Sentry.Common.Logging.Logger.Debug($"Completed MultipartUpload UploadID: {uploadId}, with response status {mRsp.HttpStatusCode}");

            versionId = mRsp.VersionId;
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
                Logger.Info($"No. of objects successfully deleted = {dosRsp.DeletedObjects.Count}");

                foreach (DeletedObject dobj in dosRsp.DeletedObjects)
                {
                    ObjectKeyVersion newItem = new ObjectKeyVersion
                    {
                        key = dobj.Key,
                        versionId = dobj.VersionId
                    };
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
                Logger.Error($"Successfully deleted = {dosRsp.DeletedObjects.Count} : Failed to Delete = {dosRsp.DeleteErrors.Count}", new Exception(sb.ToString()));
                throw new Exception($"Failed DeleteMultipleS3keys: Failed to Delete {dosRsp.DeleteErrors.Count} keys", new Exception(sb.ToString()));
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
        public void DeleteMulitpleS3keys(List<string> keys)
        {
            List<ObjectKeyVersion> keyList = new List<ObjectKeyVersion>();
            foreach (string key in keys)
            {
                keyList.Add(new ObjectKeyVersion()
                {
                    key = key,
                    versionId = null
                });
            }

            DeleteMultipleS3keys(keyList);
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

        public Dictionary<string,string> GetObjectMetadata(string key, string versionId = null)
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

        public S3FileInfo GetFileInfo(string key)
        {
            S3DirectoryInfo s3Root = new S3DirectoryInfo(S3Client, Configuration.Config.GetHostSetting("AWSRootBucket"));
            S3FileInfo outfile = s3Root.GetFile(key);

            return outfile;
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
        public IList<string> ListObjects(string bucket, string prefix, List<KeyValuePair<string, string>> tagList = null)
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
                foreach (S3Object obj in listResponse.S3Objects.OrderBy(o => o.LastModified))
                {
                    //Remove prefix object (folder)
                    if (obj.Key != prefix)
                    {
                        //filter list based on tags supplied
                        if (tagList != null)
                        {
                            //if all key\values within incoming tagList exist on the incoming object, then add to objectlist
                            if (!tagList.Except(GetObjectTags(bucket, obj.Key)).Any())
                            {
                                objectlist.Add(obj.Key);
                            }
                            else
                            {
                                Logger.Info($"S3 object filtered by ListObject (Does not contain all tags) - key:{obj.Key}");
                            }
                        }
                        else
                        {
                            objectlist.Add(obj.Key);
                        }                        
                    }                    
                }
                // Set the marker property
                listRequest.Marker = listResponse.NextMarker;
            } while (listResponse.IsTruncated);

            return objectlist;
        }       

        public string CopyObject(string srcBucket, string srcKey, string destBucket, string destKey)
        {

            Dictionary<string, string> resp = GetObjectMetadata(srcKey);

            long objectSize = Convert.ToInt64(resp["ContentLength"]);

            //Copy object file size upper limit is 5GB, if larger use multipartcopy command.
            if (objectSize > 5 * (long)Math.Pow(2, 30))
            {
                Logger.Info($"Using MultiPartCopy method - FileSize({objectSize})");
                MultiPartCopy(srcKey, destKey).Wait();

                return versionId;
            }
            else
            {
                Logger.Info($"Using MultiPartCopy method - FileSize({objectSize})");

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
        }

        public List<KeyValuePair<string, string>> GetObjectTags(string bucket, string key, string versionId = null)
        {
            GetObjectTaggingRequest tagReq = new GetObjectTaggingRequest()
            {
                BucketName = bucket,
                Key = key,
                VersionId = versionId
            };

            GetObjectTaggingResponse tagResp = S3Client.GetObjectTagging(tagReq);

            if (tagResp.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                List<KeyValuePair<string, string>> tags = new List<KeyValuePair<string, string>>();

                foreach (Tag tag in tagResp.Tagging)
                {
                    tags.Add(new KeyValuePair<string, string>(tag.Key, tag.Value));
                }
                return tags;
            }
            else
            {
                throw new AmazonS3Exception($"Error retrieving object tags - HttpStatusCode{tagResp.HttpStatusCode}");
            }
        }

        /// <summary>
        /// Adds tags to given object.  Duplicate tags will be disregarded.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="tagsToAdd"></param>
        /// <param name="versionId"></param>
        public void AddObjectTag(string bucket, string key, List<KeyValuePair<string, string>> tagsToAdd, string versionId = null)
        {
            List<KeyValuePair<string, string>> newTags;
            List<KeyValuePair<string, string>> currentTags;

            //Retrieve current tags on object
            currentTags = GetObjectTags(bucket, key, versionId);

            //Determine, of the tags requested to be added, do not already exits on the object
            newTags = tagsToAdd.Except(currentTags).ToList();

            if (newTags != null)
            {
                try
                {
                    Tagging newTagSet = new Tagging();
                    List<Tag> tags = new List<Tag>();

                    //Add non-duplicate tags to current tag list
                    foreach (KeyValuePair<string, string> tag in newTags)
                    {
                        currentTags.Add(tag);
                    }

                    //Convert to TagSet
                    foreach (KeyValuePair<string, string> tag in currentTags)
                    {
                        tags.Add(new Tag { Key = tag.Key, Value = tag.Value });
                    }

                    //Add tag list to TagSet
                    newTagSet.TagSet = tags;

                    //Create put tagging request
                    PutObjectTaggingRequest putTagReq = new PutObjectTaggingRequest()
                    {
                        BucketName = bucket,
                        Key = key,
                        VersionId = versionId,
                        Tagging = newTagSet
                    };

                    PutObjectTaggingResponse putTagResp = S3Client.PutObjectTagging(putTagReq);

                    if (putTagResp.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Logger.Error($"Error Encountered during PutObjectTaggingResponse - HttpStatusCode:{putTagResp.HttpStatusCode}");
                        throw new AmazonS3Exception($"Error Encountered during PutObjectTaggingResponse - HttpStatusCode:{putTagResp.HttpStatusCode}");
                    }
                }
                catch (AmazonS3Exception ex)
                {
                    Logger.Error("Error Encountered during PutObjectTaggingResponse", ex);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error Encountered during PutObjectTaggingResponse", ex);
                    throw;
                }
            }
        }

        public void DeleteParquetFilesByStorageCode(string storageCode)
        {
            S3DirectoryInfo S3DirectoryToDelete = new S3DirectoryInfo(S3Client, Configuration.Config.GetHostSetting("AWSRootBucket"), $"parquet/{Configuration.Config.GetHostSetting("S3DataPrefix")}{storageCode}");
            S3DirectoryToDelete.Delete(true);
        }

        public void DeleteS3Prefix(string prefix)
        {
            List<ObjectKeyVersion> s3Keys = ConvertToObjectKeyVersion(ListObjects(Configuration.Config.GetHostSetting("AWSRootBucket"), prefix).ToList());
            if (s3Keys.Count == 0)
            {
                Logger.Info($"deleteS3Prefix-nofilesdetected - prefix:{prefix}");
            }
            else
            {
                DeleteMultipleS3keys(s3Keys);
            }
        }

        public void DeleteS3Prefix(List<string> prefixList)
        {
            foreach (string prefix in prefixList)
            {
                DeleteS3Prefix(prefix);
            }
        }

        #region Helpers

        private KeyVersion ToKeyVersion (ObjectKeyVersion input)
        {
            KeyVersion key = new KeyVersion();
            key.Key = input.key;
            key.VersionId = input.versionId;
            return key;
        }
        private List<List<string>> GetPartList(List<string> sourceKeys)
        {
            throw new NotImplementedException();
        }

        private int GetObjectSize(string key)
        {
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
        
        private List<ObjectKeyVersion> ConvertToObjectKeyVersion(List<string> keyList)
        {
            List<ObjectKeyVersion> keyVersionList = new List<ObjectKeyVersion>();
            foreach (var key in keyList)
            {
                keyVersionList.Add(new ObjectKeyVersion()
                {
                    key = key,
                    versionId = null
                });
            }

            return keyVersionList;
        }
        #endregion
        
    }
}

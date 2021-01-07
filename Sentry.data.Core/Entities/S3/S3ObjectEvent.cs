using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.S3
{
    /// <summary>
    /// Used when eventName = ObjectCreated:Put
    /// </summary>
    public class S3ObjectEvent
    {
        public string eventVersion { get; set; }
        public string eventSource { get; set; }
        public string awsRegion { get; set; }
        /// <summary>
        /// The time, in ISO-8601 format, for example, 1970-01-01T00:00:00.000Z, when S3 finished processing the request
        /// </summary>
        public DateTime eventTime { get; set; }
        public string eventName { get; set; }
        public Useridentity userIdentity { get; set; }
        public Requestparameters requestParameters { get; set; }
        public Responseelements responseElements { get; set; }
        public S3 s3 { get; set; }
    }

    public class Useridentity
    {
        /// <summary>
        /// Amazon-customer-ID-of-the-user-who-caused-the-event
        /// </summary>
        public string principalId { get; set; }
    }

    public class Requestparameters
    {
        /// <summary>
        /// ip-address-where-request-came-from
        /// </summary>
        public string sourceIPAddress { get; set; }
    }

    public class Responseelements
    {
        /// <summary>
        /// Amazon S3 generated request ID
        /// </summary>
        public string xamzrequestid { get; set; }
        /// <summary>
        /// Amazon S3 host that processed the request
        /// </summary>
        public string xamzid2 { get; set; }
    }

    public class S3
    {
        public string s3SchemaVersion { get; set; }
        /// <summary>
        /// ID found in the bucket notification configuration
        /// </summary>
        public string configurationId { get; set; }
        public Bucket bucket { get; set; }
        [JsonProperty("object")]
        public Object Object { get; set; }
    }

    public class Bucket
    {
        /// <summary>
        /// bucket-name
        /// </summary>
        public string name { get; set; }
        public Owneridentity ownerIdentity { get; set; }
        /// <summary>
        /// bucket-ARN
        /// </summary>
        public string arn { get; set; }
    }

    public class Owneridentity
    {
        /// <summary>
        /// Amazon-customer-ID-of-the-bucket-owner
        /// </summary>
        public string principalId { get; set; }
    }

    public class Object
    {
        /// <summary>
        /// object-key
        /// </summary>
        public string key { get; set; }
        /// <summary>
        /// object-size
        /// </summary>
        public long size { get; set; }
        /// <summary>
        /// object eTag
        /// </summary>
        public string eTag { get; set; }
        /// <summary>
        /// object version if bucket is versioning-enabled, otherwise null
        /// </summary>
        public string versionId { get; set; }
        /// <summary>
        /// a string representation of a hexadecimal value used to determine event sequence, only used with PUTs and DELETEs
        /// </summary>
        public string sequencer { get; set; }
    }

}

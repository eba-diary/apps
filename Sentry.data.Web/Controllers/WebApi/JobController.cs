using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Job;
using Sentry.data.Core.Entities.Livy;
using Sentry.data.Core.Exceptions;
using Sentry.data.Web.Extensions;
using Sentry.data.Web.Models.ApiModels.Job;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_JOB)]
    [WebApiAuthorizeUseApp]
    public class JobController : BaseWebApiController
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IJobService _jobService;
        private readonly IApacheLivyProvider _apacheLivyProvider;

        public JobController(IDatasetContext datasetContext, IJobService jobService, 
            IApacheLivyProvider apacheLivyProvider, IDataFeatures dataFeatures)
        {
            _datasetContext = datasetContext;
            _jobService = jobService;
            _apacheLivyProvider = apacheLivyProvider;
        }
        /// <summary>
        /// Gets all Jobs
        /// </summary>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("")]
        public IHttpActionResult GetJobs()
        {
            return StatusCode(HttpStatusCode.NoContent);
            // return Ok(_datasetContext.Jobs.Select(x => x.JobOptions).ToList());
        }

        /// <summary>
        /// Gets a job by Id
        /// </summary>
        /// <param name="JobId"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("{JobId}")]
        public IHttpActionResult GetJob(int JobId)
        {
            return StatusCode(HttpStatusCode.NoContent);
            //RetrieverJob job = _datasetContext.GetById<RetrieverJob>(JobId);

            //return Ok(job.JobOptions);
        }

        /// <summary>
        /// Gets all submissions from a job
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="resultlimit">Number of results to return. Default is 100</param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SubmissionModel>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [Route("{jobid}/submission")]
        public IHttpActionResult GetJobSubmissions(int jobid, int resultlimit = 100)
        {
            try
            {
                List<SubmissionModel> SubmissionList = _jobService.GetJobSubmissions(jobid).OrderByDescending(o => o.Created).Take(resultlimit).ToList().ToSubmissionModel();

                return Ok(SubmissionList);
            }
            catch (JobNotFoundException)
            {
                return Content(HttpStatusCode.NotFound, "Job not found");
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        /// <summary>
        /// Gets all DFS from a job
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<DfsMonitorModel>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [Route("DFSMonitorList")]
        public IHttpActionResult GetDfsMonitorList(string namedEnvironment = null)
        {
            try
            {
                List<DfsMonitorDto> dtos = _jobService.GetDfsRetrieverJobs(namedEnvironment);
                List<DfsMonitorModel> models = dtos.Select(x => x.ToModel()).ToList();
                return Ok(models);
            }
            catch (Exception ex)
            {
                Logger.Error("<jobcontroller-getdfsmonitorlist> - Failed retrieving monitor list", ex);
                return InternalServerError(ex);
            }
        }


        /// <summary>
        /// Get submission detail information
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="submissionId"></param>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SubmissionDetailModel))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [Route("{jobid}/submission/{submissionId}")]
        public IHttpActionResult GetJobSubmission(int jobid, int submissionId)
        {
            try
            {
                Submission sub = _jobService.GetJobSubmissions(jobid, submissionId).FirstOrDefault();

                if (sub == null)
                {
                    Content(HttpStatusCode.NotFound, "Submission not found");
                }

                SubmissionDetailModel model = ToSubmissionDetailModel(sub);

                return Ok(model);
            }
            catch (JobNotFoundException)
            {
                return Content(HttpStatusCode.NotFound, "Job not found");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// gets submissions with optional query parameters (status and asOfDate)
        /// </summary>
        /// <param name="status"></param>
        /// <param name="asOfDate"></param>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("submissions")]
        public IHttpActionResult GetSubmissions(string status = "", string asOfDate = "")
        {
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Get a jobs specific configuration
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("{jobId}/configurations/{configId}")]
        public IHttpActionResult GetJobConfiguration(int configId)
        {
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// get all configurations from a job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("{jobId}/configurations")]
        public IHttpActionResult GetJobConfigurations(int jobId)
        {
            return StatusCode(HttpStatusCode.NoContent);
        }




        //Does this need to be moved to the new ApplicationController?
        //[HttpPost]
        //[AuthorizeByPermission(PermissionNames.AdminUser)]
        //public IHttpActionResult CreateDataSource()
        //{
        //    SourceOptions options = new SourceOptions()
        //    {
        //        JarFile = @"/apps/local/SentrySparkApp/SentrySparkApp-0.1.0-DEV-1.jar",
        //        ClassName = "com.sentry.service.SdapCommonService",
        //        JarDepenencies = new string[]
        //        {
        //            "/apps/local/lib/unused-1.0.0.jar","/apps/local/lib/validation-api-1.1.0.Final.jar","/apps/local/lib/xbean-asm5-shaded-4.4.jar","/apps/local/lib/xercesImpl-2.9.1.jar","/apps/local/lib/xml-apis-1.3.04.jar","/apps/local/lib/xmlbeans-2.6.0.jar","/apps/local/lib/xmlenc-0.52.jar","/apps/local/lib/xz-1.0.jar","/apps/local/lib/zkclient-0.8.jar","/apps/local/lib/zookeeper-3.4.6-tests.jar","/apps/local/lib/sentry-enterprise-logging-1.0.7.jar","/apps/local/lib/sentry-kafka-messagediagnostics-1.0.2-RC-5.jar","/apps/local/lib/servlet-api-2.5.jar","/apps/local/lib/si-kafka-2.2.2-RC-4.jar","/apps/local/lib/slf4j-api-1.7.22.jar","/apps/local/lib/slf4j-log4j12-1.7.21.jar","/apps/local/lib/snappy-java-1.1.2.6.jar","/apps/local/lib/spark-catalyst_2.11-2.2.0-tests.jar","/apps/local/lib/spark-core_2.11-2.2.0-tests.jar","/apps/local/lib/spark-launcher_2.11-2.2.0.jar","/apps/local/lib/spark-network-common_2.11-2.2.0-tests.jar","/apps/local/lib/spark-network-shuffle_2.11-2.2.0.jar","/apps/local/lib/spark-sketch_2.11-2.2.0.jar","/apps/local/lib/spark-sql_2.11-2.2.0.jar","/apps/local/lib/spark-sql-kafka-0-10_2.11-2.2.0.jar","/apps/local/lib/spark-streaming_2.11-2.2.0.jar","/apps/local/lib/spark-streaming-kafka-0-10_2.11-2.2.0.jar","/apps/local/lib/spark-tags_2.11-2.2.0-tests.jar","/apps/local/lib/spark-unsafe_2.11-2.2.0.jar","/apps/local/lib/sqljdbc4-4.0.2206.100.jar","/apps/local/lib/stax-api-1.0.1.jar","/apps/local/lib/stax-api-1.0-2.jar","/apps/local/lib/stream-2.7.0.jar","/apps/local/lib/univocity-parsers-2.2.1.jar","/apps/local/lib/activation-1.1.1.jar","/apps/local/lib/antlr4-runtime-4.5.3.jar","/apps/local/lib/aopalliance-1.0.jar","/apps/local/lib/aopalliance-repackaged-2.4.0-b34.jar","/apps/local/lib/apacheds-i18n-2.0.0-M15.jar","/apps/local/lib/apacheds-kerberos-codec-2.0.0-M15.jar","/apps/local/lib/api-asn1-api-1.0.0-M20.jar","/apps/local/lib/api-util-1.0.0-M20.jar","/apps/local/lib/asm-3.2.jar","/apps/local/lib/avro-1.7.7-tests.jar","/apps/local/lib/avro-ipc-1.7.7-tests.jar","/apps/local/lib/avro-mapred-1.7.7-hadoop2.jar","/apps/local/lib/aws-java-sdk-1.10.6.jar","/apps/local/lib/aws-java-sdk-autoscaling-1.10.6.jar","/apps/local/lib/aws-java-sdk-cloudformation-1.10.6.jar","/apps/local/lib/aws-java-sdk-cloudfront-1.10.6.jar","/apps/local/lib/aws-java-sdk-cloudhsm-1.10.6.jar","/apps/local/lib/aws-java-sdk-cloudsearch-1.10.6.jar","/apps/local/lib/aws-java-sdk-cloudtrail-1.10.6.jar","/apps/local/lib/aws-java-sdk-cloudwatch-1.10.6.jar","/apps/local/lib/aws-java-sdk-cloudwatchmetrics-1.10.6.jar","/apps/local/lib/aws-java-sdk-codecommit-1.10.6.jar","/apps/local/lib/aws-java-sdk-codedeploy-1.10.6.jar","/apps/local/lib/aws-java-sdk-codepipeline-1.10.6.jar","/apps/local/lib/aws-java-sdk-cognitoidentity-1.10.6.jar","/apps/local/lib/aws-java-sdk-cognitosync-1.10.6.jar","/apps/local/lib/aws-java-sdk-config-1.10.6.jar","/apps/local/lib/aws-java-sdk-core-1.10.6.jar","/apps/local/lib/aws-java-sdk-datapipeline-1.10.6.jar","/apps/local/lib/aws-java-sdk-devicefarm-1.10.6.jar","/apps/local/lib/aws-java-sdk-directconnect-1.10.6.jar","/apps/local/lib/aws-java-sdk-directory-1.10.6.jar","/apps/local/lib/aws-java-sdk-dynamodb-1.10.6.jar","/apps/local/lib/aws-java-sdk-ec2-1.10.6.jar","/apps/local/lib/aws-java-sdk-ecs-1.10.6.jar","/apps/local/lib/aws-java-sdk-efs-1.10.6.jar","/apps/local/lib/aws-java-sdk-elasticache-1.10.6.jar","/apps/local/lib/aws-java-sdk-elasticbeanstalk-1.10.6.jar","/apps/local/lib/aws-java-sdk-elasticloadbalancing-1.10.6.jar","/apps/local/lib/aws-java-sdk-elastictranscoder-1.10.6.jar","/apps/local/lib/aws-java-sdk-emr-1.10.6.jar","/apps/local/lib/aws-java-sdk-glacier-1.10.6.jar","/apps/local/lib/aws-java-sdk-iam-1.10.6.jar","/apps/local/lib/aws-java-sdk-importexport-1.10.6.jar","/apps/local/lib/aws-java-sdk-kinesis-1.10.6.jar","/apps/local/lib/aws-java-sdk-kms-1.10.6.jar","/apps/local/lib/aws-java-sdk-lambda-1.10.6.jar","/apps/local/lib/aws-java-sdk-logs-1.10.6.jar","/apps/local/lib/aws-java-sdk-machinelearning-1.10.6.jar","/apps/local/lib/aws-java-sdk-opsworks-1.10.6.jar","/apps/local/lib/aws-java-sdk-rds-1.10.6.jar","/apps/local/lib/aws-java-sdk-redshift-1.10.6.jar","/apps/local/lib/aws-java-sdk-route53-1.10.6.jar","/apps/local/lib/aws-java-sdk-s3-1.10.6.jar","/apps/local/lib/aws-java-sdk-ses-1.10.6.jar","/apps/local/lib/aws-java-sdk-simpledb-1.10.6.jar","/apps/local/lib/aws-java-sdk-simpleworkflow-1.10.6.jar","/apps/local/lib/aws-java-sdk-sns-1.10.6.jar","/apps/local/lib/aws-java-sdk-sqs-1.10.6.jar","/apps/local/lib/aws-java-sdk-ssm-1.10.6.jar","/apps/local/lib/aws-java-sdk-storagegateway-1.10.6.jar","/apps/local/lib/aws-java-sdk-sts-1.10.6.jar","/apps/local/lib/aws-java-sdk-support-1.10.6.jar","/apps/local/lib/aws-java-sdk-swf-libraries-1.10.6.jar","/apps/local/lib/aws-java-sdk-workspaces-1.10.6.jar","/apps/local/lib/base64-2.3.8.jar","/apps/local/lib/bcprov-jdk15on-1.51.jar","/apps/local/lib/c3p0-0.9.5.2.jar","/apps/local/lib/cglib-2.2.1-v20090111.jar","/apps/local/lib/chill_2.11-0.8.0.jar","/apps/local/lib/chill-java-0.8.0.jar","/apps/local/lib/commons-beanutils-1.7.0.jar","/apps/local/lib/commons-beanutils-core-1.8.0.jar","/apps/local/lib/commons-cli-1.2.jar","/apps/local/lib/commons-codec-1.10.jar","/apps/local/lib/commons-collections-3.2.2.jar","/apps/local/lib/commons-compiler-3.0.0.jar","/apps/local/lib/commons-compress-1.4.1.jar","/apps/local/lib/commons-configuration-1.6.jar","/apps/local/lib/commons-crypto-1.0.0.jar","/apps/local/lib/commons-daemon-1.0.13.jar","/apps/local/lib/commons-dbutils-1.6.jar","/apps/local/lib/commons-digester-1.8.jar","/apps/local/lib/commons-httpclient-3.1.jar","/apps/local/lib/commons-io-2.6.jar","/apps/local/lib/commons-lang-2.6.jar","/apps/local/lib/commons-lang3-3.7.jar","/apps/local/lib/commons-logging-1.1.3.jar","/apps/local/lib/commons-math3-3.4.1.jar","/apps/local/lib/commons-net-3.1.jar","/apps/local/lib/commons-pool-1.6.jar","/apps/local/lib/compress-lzf-1.0.3.jar","/apps/local/lib/connect-api-0.10.0.1.jar","/apps/local/lib/connect-json-0.10.0.1.jar","/apps/local/lib/curator-client-2.7.1.jar","/apps/local/lib/curator-framework-2.7.1.jar","/apps/local/lib/curator-recipes-2.7.1.jar","/apps/local/lib/curvesapi-1.03.jar","/apps/local/lib/gson-2.2.4.jar","/apps/local/lib/guava-11.0.2.jar","/apps/local/lib/guice-3.0.jar","/apps/local/lib/hadoop-annotations-2.7.3.jar","/apps/local/lib/hadoop-auth-2.7.3-tests.jar","/apps/local/lib/hadoop-client-2.7.3.jar","/apps/local/lib/hadoop-common-2.7.3.jar","/apps/local/lib/hadoop-hdfs-2.7.3.jar","/apps/local/lib/hadoop-mapreduce-client-app-2.7.3.jar","/apps/local/lib/hadoop-mapreduce-client-common-2.7.3.jar","/apps/local/lib/hadoop-mapreduce-client-core-2.7.3.jar","/apps/local/lib/hadoop-mapreduce-client-jobclient-2.7.3.jar","/apps/local/lib/hadoop-yarn-api-2.7.3.jar","/apps/local/lib/hadoop-mapreduce-client-shuffle-2.7.3.jar","/apps/local/lib/hadoop-yarn-client-2.7.3.jar","/apps/local/lib/hadoop-yarn-common-2.7.3-tests.jar","/apps/local/lib/hadoop-yarn-server-common-2.7.3.jar","/apps/local/lib/hk2-api-2.4.0-b34.jar","/apps/local/lib/hk2-locator-2.4.0-b34.jar","/apps/local/lib/hk2-utils-2.4.0-b34.jar","/apps/local/lib/htrace-core-3.1.0-incubating.jar","/apps/local/lib/httpclient-4.5.2.jar","/apps/local/lib/httpcore-4.4.4.jar","/apps/local/lib/ivy-2.4.0.jar","/apps/local/lib/jackson-annotations-2.6.5.jar","/apps/local/lib/jackson-core-2.6.5.jar","/apps/local/lib/jackson-core-asl-1.9.13.jar","/apps/local/lib/jackson-databind-2.6.5.jar","/apps/local/lib/jackson-jaxrs-1.9.13.jar","/apps/local/lib/jackson-mapper-asl-1.9.13.jar","/apps/local/lib/jackson-module-paranamer-2.6.5.jar","/apps/local/lib/jackson-module-scala_2.11-2.6.5.jar","/apps/local/lib/jackson-xc-1.9.13.jar","/apps/local/lib/janino-3.0.0.jar","/apps/local/lib/javassist-3.18.1-GA.jar","/apps/local/lib/javax.annotation-api-1.2.jar","/apps/local/lib/javax.inject-1.jar","/apps/local/lib/javax.inject-2.4.0-b34.jar","/apps/local/lib/javax.servlet-api-3.1.0.jar","/apps/local/lib/javax.ws.rs-api-2.0.1.jar","/apps/local/lib/java-xmlbuilder-1.0.jar","/apps/local/lib/jaxb-api-2.2.2.jar","/apps/local/lib/jaxb-impl-2.2.3-1.jar","/apps/local/lib/jcl-over-slf4j-1.7.16.jar","/apps/local/lib/jersey-client-1.9.jar","/apps/local/lib/jersey-client-2.22.2.jar","/apps/local/lib/jersey-common-2.22.2.jar","/apps/local/lib/jersey-container-servlet-2.22.2.jar","/apps/local/lib/jersey-container-servlet-core-2.22.2.jar","/apps/local/lib/jersey-core-1.9.jar","/apps/local/lib/jersey-guava-2.22.2.jar","/apps/local/lib/jersey-guice-1.9.jar","/apps/local/lib/jersey-json-1.9.jar","/apps/local/lib/jersey-media-jaxb-2.22.2.jar","/apps/local/lib/jersey-server-1.9.jar","/apps/local/lib/jersey-server-2.22.2.jar","/apps/local/lib/jets3t-0.9.3.jar","/apps/local/lib/jettison-1.1.jar","/apps/local/lib/jetty-6.1.26.jar","/apps/local/lib/jetty-util-6.1.26.jar","/apps/local/lib/jline-0.9.94.jar","/apps/local/lib/joda-time-2.8.1.jar","/apps/local/lib/jopt-simple-4.9.jar","/apps/local/lib/jsch-0.1.42.jar","/apps/local/lib/json4s-ast_2.11-3.2.11.jar","/apps/local/lib/json4s-core_2.11-3.2.11.jar","/apps/local/lib/json4s-jackson_2.11-3.2.11.jar","/apps/local/lib/json-20160810.jar","/apps/local/lib/jsp-api-2.1.jar","/apps/local/lib/jsr305-3.0.0.jar","/apps/local/lib/jul-to-slf4j-1.7.16.jar","/apps/local/lib/junit-3.8.1.jar","/apps/local/lib/kafka_2.10-0.10.0.1.jar","/apps/local/lib/kafka_2.11-0.10.0.1.jar","/apps/local/lib/kafka-clients-0.10.2.2.jar","/apps/local/lib/kafka-streams-0.10.0.1.jar","/apps/local/lib/kryo-shaded-3.0.3.jar","/apps/local/lib/leveldbjni-all-1.8.jar","/apps/local/lib/log4j-1.2.17.jar","/apps/local/lib/log4j-1.2-api-2.11.1.jar","/apps/local/lib/log4j-api-2.11.1.jar","/apps/local/lib/log4j-core-2.11.1.jar","/apps/local/lib/lz4-1.3.0.jar","/apps/local/lib/lz4-java-1.4.1.jar","/apps/local/lib/mail-1.4.7.jar","/apps/local/lib/mchange-commons-java-0.2.11.jar","/apps/local/lib/metrics-core-2.2.0.jar","/apps/local/lib/metrics-core-3.1.2.jar","/apps/local/lib/metrics-graphite-3.1.2.jar","/apps/local/lib/metrics-json-3.1.2.jar","/apps/local/lib/metrics-jvm-3.1.2.jar","/apps/local/lib/minlog-1.3.0.jar","/apps/local/lib/mx4j-3.0.2.jar","/apps/local/lib/netty-3.9.9.Final.jar","/apps/local/lib/netty-all-4.0.43.Final.jar","/apps/local/lib/objenesis-2.1.jar","/apps/local/lib/oro-2.0.8.jar","/apps/local/lib/osgi-resource-locator-1.0.1.jar","/apps/local/lib/paranamer-2.6.jar","/apps/local/lib/parquet-column-1.10.0.jar","/apps/local/lib/parquet-common-1.10.0.jar","/apps/local/lib/parquet-encoding-1.10.0-tests.jar","/apps/local/lib/parquet-format-2.4.0.jar","/apps/local/lib/parquet-hadoop-1.10.0.jar","/apps/local/lib/parquet-jackson-1.10.0.jar","/apps/local/lib/poi-3.14.jar","/apps/local/lib/poi-ooxml-3.14.jar","/apps/local/lib/poi-ooxml-schemas-3.14.jar","/apps/local/lib/protobuf-java-2.5.0.jar","/apps/local/lib/py4j-0.10.4.jar","/apps/local/lib/pyrolite-4.13.jar","/apps/local/lib/RoaringBitmap-0.5.11.jar","/apps/local/lib/rocksdbjni-4.8.0.jar","/apps/local/lib/scala-compiler-2.11.8.jar","/apps/local/lib/scala-library-2.11.8.jar","/apps/local/lib/scalap-2.11.8.jar","/apps/local/lib/scala-reflect-2.11.8.jar","/apps/local/lib/scala-xml_2.11-1.0.4.jar","/apps/local/lib/scala-parser-combinators_2.11-1.0.4.jar"
        //        }
        //    };

        //    DataSource source = new JavaAppSource()
        //    {
        //        Name = "SentrySparkApp02",
        //        Description = "General Java Application for DSC processing on Spark",
        //        Options = options,
        //        BaseUri = new Uri("hdfs://apps/local/SentrySparkApp/SentrySparkApp-0.1.0-DEV-1.jar"),
        //        SourceAuthType = _datasetContext.AuthTypes.Where(w => w is AnonymousAuthentication).FirstOrDefault()
        //    };

        //    _datasetContext.Merge(source);
        //    _datasetContext.SaveChanges();

        //    return Ok();
        //}



        /// <summary>
        /// update a Job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("{jobId}")]
        public IHttpActionResult UpdateJob(int jobId)
        {
            return NoContent();
        }

        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="App_Id"></param>
        /// <param name="App_Guid"></param>
        /// <param name="Schedule"></param>
        /// <param name="isEnabled"></param>
        /// <param name="javaOptionsOverride"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(int))]
        [Route("{App_Id}/{App_Guid}/{Schedule}/{isEnabled}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public IHttpActionResult CreateNewJob(
            int App_Id, 
            int? App_Guid = null, 
            string Schedule = null, 
            Boolean isEnabled = false, 
            [FromBody] JavaOptionsOverride javaOptionsOverride = null)
        {

            Dataset ds = _datasetContext.GetById<Dataset>(App_Id);

            //JavaOptions jo = new JavaOptions()
            //{
            //    DriverCores = javaOptionsOverride.DriverCores,
            //    DriverMemory = javaOptionsOverride.DriverMemory,
            //    ExecutorCores = javaOptionsOverride.ExecutorCores,
            //    ExecutorMemory = javaOptionsOverride.ExecutorMemory,
            //    NumExecutors = javaOptionsOverride.NumExecutors,

            //    Arguments = javaOptionsOverride.Arguments,

            //    ConfigurationParameters = javaOptionsOverride.ConfigurationParameters
            //};

            RetrieverJobOptions rjo = new RetrieverJobOptions()
            {
                JavaAppOptions = new RetrieverJobOptions.JavaOptions()
                {
                    DriverCores = javaOptionsOverride.DriverCores,
                    DriverMemory = javaOptionsOverride.DriverMemory,
                    ExecutorCores = javaOptionsOverride.ExecutorCores,
                    ExecutorMemory = javaOptionsOverride.ExecutorMemory,
                    NumExecutors = javaOptionsOverride.NumExecutors,

                    Arguments = javaOptionsOverride.Arguments,

                    ConfigurationParameters = javaOptionsOverride.ConfigurationParameters
                }
            };

            RetrieverJob job = new RetrieverJob()
            {
                Schedule = Schedule,
                JobOptions = rjo,
                DataSource = _datasetContext.GetById<DataSource>(72),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                DatasetConfig = ds.DatasetFileConfigs.FirstOrDefault(),
                IsEnabled = isEnabled,
                ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active
            };

            RetrieverJob newJob = _datasetContext.Merge(job);
            _datasetContext.SaveChanges();

            return Ok(newJob.Id);
        }

        /// <summary>
        /// Submits a job for run
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="JobGuid"></param>
        /// <param name="javaOptionsOverride"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(string))]
        [SwaggerResponse(HttpStatusCode.NotFound, null, typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [Route("{JobId}/submit/{JobGuid}")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> SubmitJob(int JobId, Guid JobGuid, [FromBody] JavaOptionsOverride javaOptionsOverride)
        {
            string methodName = MethodBase.GetCurrentMethod().Name.ToLower();
            
            try
            {
                Logger.Info($"Start method <{methodName}> ");
                if (JobId == 0)
                {
                    return BadRequest("JobId parameter required");
                }

                if (JobGuid == Guid.Empty)
                {
                    return BadRequest("JobGuid parameter required");
                }

                /*
                 * Add flowexecutionguid context variable
                 */
                string flowExecutionGuid = (javaOptionsOverride != null && javaOptionsOverride.FlowExecutionGuid != null) ? javaOptionsOverride.FlowExecutionGuid : "00000000000000000";
                Logger.AddContextVariable(new TextVariable("flowexecutionguid", flowExecutionGuid));

                /*
                 * Add runinstanceguid context variable
                 */
                string runInstanceGuid = (javaOptionsOverride != null && javaOptionsOverride.RunInstanceGuid != null) ? javaOptionsOverride.RunInstanceGuid : "00000000000000000";
                Logger.AddContextVariable(new TextVariable("runinstanceguid", runInstanceGuid));
                
                Logger.Info($"Start method <{methodName}>  JobId: {JobId} JobGuid: {JobGuid}");

                Logger.Info(javaOptionsOverride != null
                    ? $"JobId: {JobId} JobGuid: {JobGuid}  JavaOptionsOverride: {JsonConvert.SerializeObject(javaOptionsOverride)}"
                    : $"JobId: {JobId} JobGuid: {JobGuid}  JavaOptionsOverride: Not supplied");

                JavaOptionsOverrideDto dto = javaOptionsOverride.ToDto();

                try
                {
                    HttpResponseMessage response  = await _jobService.SubmitApacheLivyJobAsync(JobId, JobGuid, dto);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(response);
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        Logger.Debug($"BadRequest from Spark (Job\\Submit) - JobId:{JobId} JobGuid:{JobGuid} javaOptionsOverride:{JsonConvert.SerializeObject(dto)}");
                        return BadRequest(response.Content.ReadAsStringAsync().Result);
                    }
                    else if (response.StatusCode == HttpStatusCode.BadGateway)
                    {
                        return Content(HttpStatusCode.BadGateway, "Apache Livy API unavailable");
                    }
                    else if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                    {
                        return Content(HttpStatusCode.GatewayTimeout, "Apache Livy did not response in timely fashion");
                    }
                    else
                    {
                        Logger.Debug($"Status NotFound (Job\\Submit) - JobId:{JobId} JobGuid:{JobGuid} javaOptionsOverride:{JsonConvert.SerializeObject(javaOptionsOverride)}");
                        return NotFound();
                    }
                }
                catch (JobNotFoundException ex)
                {
                    return Content(HttpStatusCode.NotFound, ex.Message);
                }
                catch(Exception argEx) when (argEx is ArgumentNullException || argEx is ArgumentOutOfRangeException)
                {
                    return BadRequest(argEx.Message);
                }
            }
            catch (Exception ex)
            {

                Logger.Error($"Internal Error (Job\\Submit) - JobId:{JobId} JobGuid:{JobGuid} javaOptionsOverride:{JsonConvert.SerializeObject(javaOptionsOverride)}", ex);
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [Route("batches")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetBatchList()
        {
            try
            {
                HttpResponseMessage response = await _apacheLivyProvider.GetRequestAsync($"/batches").ConfigureAwait(false);

                string result = response.Content.ReadAsStringAsync().Result;
                string sendresult = (string.IsNullOrEmpty(result)) ? "noresultsupplied" : result;

                Logger.Debug($"getbatchstate_livyresponse statuscode:{response.StatusCode.ToString()}:::result:{sendresult}");
                
                return Ok();

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Gets a batch from a job
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="batchId"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(string))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        [Route("{jobId}/batches/{batchId}")]
        [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetBatchState(int jobId, int batchId)
        {
            try
            {
                await _jobService.GetApacheLivyBatchStatusAsync(jobId, batchId);

                return Ok();
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is LivyBatchNotFoundException)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpResponseException responseEx)
            {
                Logger.Error("<jobcontroller-getbatchstate> livy connection failed", responseEx);
                return Content(HttpStatusCode.BadGateway, "Failed to reach Livy");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Error("<jobcontroller-getbatchstate> livy connection timeout", timeoutEx);
                return Content(HttpStatusCode.GatewayTimeout, "Livy connection timeout");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private SubmissionDetailModel ToSubmissionDetailModel(Submission sub)
        {
            SubmissionDetailModel model = new SubmissionDetailModel();
            Extensions.JobExtensions.ToModel(sub, model);

            List<JobHistory> jobHistoryList = _jobService.GetJobHistoryBySubmission(sub.SubmissionId);
            JobHistory latestHistoryRecord = jobHistoryList.OrderByDescending(o => o.HistoryId).Take(1).FirstOrDefault();

            model.LastStatus = (jobHistoryList.Any()) ? latestHistoryRecord.State : "No History";
            model.JobHistory = ToModel(jobHistoryList);

            return model;
        }

        private JobHistoryModel ToModel(JobHistory dto)
        {
            JobHistoryModel model = new JobHistoryModel()
            {
                Job_Id = dto.JobId.Id,
                Batch_Id = dto.BatchId,
                Active = dto.Active,
                Created_DTM = dto.Created.ToString(),
                History_Id = dto.HistoryId,
                LivyAppId = dto.LivyAppId,
                LivyDriverLogUrl = dto.LivyDriverLogUrl,
                LivySparkUiUrl = dto.LivySparkUiUrl,
                Modified_DTM = dto.Modified.ToString(),
                LogInfo = dto.LogInfo,
                State = dto.State
            };

            return model;
        }

        private List<JobHistoryModel> ToModel(List<JobHistory> dto)
        {
            List<JobHistoryModel> modelList = new List<JobHistoryModel>();
            foreach (JobHistory history in dto)
            {
                modelList.Add(ToModel(history));
            }
            return modelList;
        }
    }
}
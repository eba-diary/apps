using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.Livy;
using Sentry.data.Core.Exceptions;
using Sentry.data.Infrastructure;
using Sentry.data.Web.Helpers;
using Sentry.WebAPI.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_QUERYTOOL)]
    public class QueryToolController : BaseWebApiController
    {
        public IAssociateInfoProvider _associateInfoProvider;
        public IDatasetContext _datasetContext;
        private UserService _userService;
        private S3ServiceProvider _s3Service;
        public LivyHelper _livy;
        public string _livyUrl;
        private IConfigService _configService;
        private ISchemaService _schemaService;
        private ISecurityService _securityService;
        private IDatasetService _datasetService;
        private string _bucket;
        private IApacheLivyProvider _apacheLivyProvider;

        public QueryToolController(IDatasetContext dsCtxt, S3ServiceProvider dsSvc, UserService userService, IAssociateInfoProvider associateInfoService, IConfigService configService,
            ISchemaService schemaService, ISecurityService securityService, IDatasetService datasetService,
            IApacheLivyProvider apacheLivyProvider)
        {
            _datasetContext = dsCtxt;
            _userService = userService;
            _s3Service = dsSvc;
            _livy = new LivyHelper(dsCtxt);
            _associateInfoProvider = associateInfoService;
            _livyUrl = Config.GetHostSetting("ApacheLivy");
            _configService = configService;
            _schemaService = schemaService;
            _securityService = securityService;
            _datasetService = datasetService;
            _apacheLivyProvider = apacheLivyProvider;
        }

        private string RootBucket
        {
            get
            {
                if (_bucket == null)
                {
                    _bucket = Config.GetHostSetting("AWS2_0RootBucket");
                }
                return _bucket;
            }
        }

        /// <summary>
        /// gets all primary livey sessions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("sessions/primary")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetPrimarySessionID()
        {
            LivyCreation lc = null;
            try
            {
                lc = _datasetContext.LivySessions.Where(x => x.ForDSC_IND == true && x.Session_NME == "Primary Data.Sentry.com Livy Session" && x.Active_IND == true).FirstOrDefault();
            }
            catch (Exception ex)
            {

            }

            if (lc == null)
            {
                lc = new LivyCreation()
                {
                    Session_NME = "Primary Data.Sentry.com Livy Session",
                    Active_IND = true,
                    ForDSC_IND = true,
                    Kind = "Python",
                    ExecutorCores = 3,
                    ExecutorMemory = "7G",
                    NumExecutors = 1,
                    HeartbeatTimeoutInSecond = 86400
                };

                IHttpActionResult creationResponse = await CreateInternalSession(lc.Kind, lc).ConfigureAwait(false);

                if (creationResponse.GetType() == typeof(OkNegotiatedContentResult<String>))
                {
                    try
                    {
                        var a = creationResponse as OkNegotiatedContentResult<String>;
                        LivySession ls = JsonConvert.DeserializeObject<LivySession>(a.Content);

                        lc.LivySession_ID = ls.id;
                        lc.Start_DTM = DateTime.Now;
                        lc.End_DTM = DateTime.Now;
                        lc.Active_IND = true;
                        _datasetContext.Merge<LivyCreation>(lc);
                        _datasetContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    return BadRequest("For some reason we couldn't start a new session.");
                }
            }


            IHttpActionResult response = await GetSession(lc.LivySession_ID).ConfigureAwait(false);

            //Reply from Livy.
            if (response.GetType() == typeof(OkNegotiatedContentResult<String>))
            {
                var a = response as OkNegotiatedContentResult<String>;
                LivyReply lr = JsonConvert.DeserializeObject<LivyReply>(a.Content);

                lr.livyURL = _livyUrl;

                return Ok(lr);
            }
            //No Reply From Livy
            else
            {
                lc.End_DTM = DateTime.Now;
                lc.Active_IND = false;
                _datasetContext.Merge<LivyCreation>(lc);
                _datasetContext.SaveChanges();

                lc = new LivyCreation()
                {
                    Session_NME = "Primary Data.Sentry.com Livy Session",
                    Active_IND = true,
                    ForDSC_IND = true,
                    Kind = "Python",
                    ExecutorCores = 3,
                    ExecutorMemory = "7G",
                    NumExecutors = 1,
                    HeartbeatTimeoutInSecond = 86400
                };

                IHttpActionResult creationResponse = await (CreateInternalSession(lc.Kind, lc));

                if (creationResponse.GetType() == typeof(OkNegotiatedContentResult<String>))
                {
                    try
                    {
                        var a = creationResponse as OkNegotiatedContentResult<String>;
                        LivySession ls = JsonConvert.DeserializeObject<LivySession>(a.Content);

                        lc.LivySession_ID = ls.id;
                        lc.Start_DTM = DateTime.Now;
                        lc.End_DTM = DateTime.Now;
                        lc.Active_IND = true;
                        _datasetContext.Merge<LivyCreation>(lc);
                        _datasetContext.SaveChanges();

                        ls.livyURL = _livyUrl;
                        return Ok(ls);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("For some reason we couldn't start a new session.");
                    }
                }
                else
                {
                    return BadRequest("For some reason we couldn't start a new session.");
                }
            }
        }

        /// <summary>
        /// creates a new session for given language
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sessions/{Language}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> CreateSession(string Language)
        {
            string json;
            switch (Language)
            {
                default:
                case "Python":
                    json = "{\"kind\": \"pyspark\"";
                    break;
                case "Scala":
                    json = "{\"kind\": \"spark\"";
                    break;
                case "R":
                    json = "{\"kind\": \"rspark\"";
                    break;
            }

            json += ", \"name\": \"DSC_" + _userService.GetCurrentUser().AssociateId + "\"";



            json += ", \"conf\": { \"spark.hadoop.fs.s3a.security.credential.provider.path\" : \"" + Config.GetHostSetting("SparkS3AKeyLocation") + "\"," +
                                    "\"spark.sql.hive.convertMetastoreParquet\" : \"false\"}";
            json += "}";


            HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _apacheLivyProvider.PostRequestAsync("/sessions", contentPost).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// creates a new internal session
        /// </summary>
        /// <param name="Language"></param>
        /// <param name="lc"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("internalSessions/{Language}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> CreateInternalSession(string Language = "", [FromBody] LivyCreation lc = null)
        {
            string json;
            switch (Language)
            {
                default:
                case "Python":
                    json = "{\"kind\": \"pyspark\"";
                    break;
                case "Scala":
                    json = "{\"kind\": \"spark\"";
                    break;
                case "R":
                    json = "{\"kind\": \"rspark\"";
                    break;
            }

            if (lc != null)
            {
                if (lc.Session_NME != null)
                {
                    json += ", \"name\": \"" + lc.Session_NME + "\"";
                }

                if (lc.Queue != null)
                {
                    json += ", \"queue\": \"" + lc.Queue + "\"";
                }

                if (lc.ExecutorCores != 0)
                {
                    json += ", \"executorCores\": " + lc.ExecutorCores + "";
                }

                if (lc.ExecutorMemory != null)
                {
                    json += ", \"executorMemory\": \"" + lc.ExecutorMemory + "\"";
                }

                if (lc.NumExecutors != 0)
                {
                    json += ", \"numExecutors\": " + lc.NumExecutors + "";
                }

                if (lc.HeartbeatTimeoutInSecond != 0)
                {
                    json += ", \"heartbeatTimeoutInSecond\": " + lc.HeartbeatTimeoutInSecond + "";
                }
            }
            else
            {
                json += ", \"name\": \"DSC_" + _userService.GetCurrentUser().AssociateId + "\"";
            }


            json += ", \"conf\": { \"spark.hadoop.fs.s3a.security.credntial.provider.path\" : \"" + Config.GetHostSetting("SparkS3AKeyLocation") + "\"," +
                                    "\"spark.sql.hive.convertMetastoreParquet\" : \"false\"}";
            json += "}";


            HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _apacheLivyProvider.PostRequestAsync("/sessions", contentPost).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// sends code to a session
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="Code"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sessions/{SessionID}/sendCode")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> SendCode(int SessionID, [FromBody] string Code)
        {
            //dynamic json = Code;
            var json = "{\"code\": \"" + Code + "\"}";

            //json = new JavaScriptSerializer().Serialize();

            HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _apacheLivyProvider.PostRequestAsync("/sessions/" + SessionID + "/statements", contentPost).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// get a session by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("sessions/name/{name}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetSessions(String name = null)
        {
            HttpResponseMessage response = await _apacheLivyProvider.GetRequestAsync("/sessions").ConfigureAwait(false);

            if (name != null && response.IsSuccessStatusCode)
            {
                LivySessionList livySessionList = JsonConvert.DeserializeObject<LivySessionList>(response.Content.ReadAsStringAsync().Result);

                return Ok(livySessionList.sessions.Where(x => x.appId.Contains(name)));
            }
            else if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest();
            }
            else
            {
                return NotFound();
            }
            
        }

        /// <summary>
        /// get session by id
        /// </summary>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("sessions/{SessionID}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetSession(int SessionID)
        {
            HttpResponseMessage response = await _apacheLivyProvider.GetRequestAsync("/sessions/" + SessionID).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest();
            }
            else
            {
                return NotFound();
            }
            
        }

        /// <summary>
        /// gets a statement from a session
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="StatementID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("sessions/{SessionID}/statements/{StatementID}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetStatement(int SessionID, int StatementID)
        {
            HttpResponseMessage response = await _apacheLivyProvider.GetRequestAsync("/sessions/" + SessionID + "/statements/" + StatementID).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest();
            }
            else
            {
                return NotFound();
            }
            
        }

        /// <summary>
        /// cancel a statement on a session
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="StatementID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sessions/{SessionID}/statements/{StatementID}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> CancelStatement(int SessionID, int StatementID)
        {
            HttpResponseMessage response = await _apacheLivyProvider.PostRequestAsync("/sessions/" + SessionID + "/statements/" + StatementID + "/cancel", content:null).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest();
            }
            else
            {
                return NotFound();
            }
            
        }

        /// <summary>
        /// deletes a session
        /// </summary>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("sessions/{SessionID}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> DeleteSession(int SessionID)
        {
            HttpResponseMessage response = await _apacheLivyProvider.DeleteRequestAsync("/sessions/" + SessionID);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest();
            }
            else
            {
                return NotFound();
            }
            
        }

        /// <summary>
        /// gets the s3 file drop location
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("files/fileDropLocation")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> FileDropLocation()
        {
            IApplicationUser user = _userService.GetCurrentUser();

            var obj = new
            {
                s3Key = RootBucket + "/" + Utilities.GenerateCustomStorageLocation(new string[] { "QueryTool/Bundle", user.AssociateId })
            };

            return Ok(obj);
        }

        /// <summary>
        /// gets a file count from the s3 drop location
        /// </summary>
        /// <param name="s3Key"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("files/{s3Key}/fileCount")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> FileCount(string s3Key)
        {
            try
            {
                return Ok(_s3Service.FindObject(s3Key).Count);
            }
            catch
            {
                return NotFound();
            }
        }

        /// <summary>
        /// gets a dataset from a file location
        /// </summary>
        /// <param name="s3Key"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("files/DownloadUrl/{*s3Key}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetDatasetFileDownloadURL(string s3Key)
        {
            var key = s3Key.Substring(0, s3Key.LastIndexOf('/') + 1);
            var fileName = s3Key.Replace(key, "");

            try
            {
                List<string> list = _s3Service.FindObject(key);

                foreach (string obj in list)
                {
                    if (obj.Contains("part-00000"))
                    {
                        return Ok(_s3Service.GetDatasetDownloadUrl(obj, null, fileName));
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        /// <summary>
        /// gets s3 key from dataset id
        /// </summary>
        /// <param name="datasetID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("datasets/{datasetID}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetS3Key(int datasetID)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(datasetID);
            UserSecurity us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());

            List<DatasetFileConfigDto> configDtoList = _configService.GetDatasetFileConfigDtoByDataset(datasetID).Where(w => !w.DeleteInd).ToList();

            List<QueryableConfig> reply = new List<QueryableConfig>();

            foreach(DatasetFileConfigDto dto in configDtoList)
            {
                FileSchemaDto schemaDto = dto.Schema;
                if (dto.FileTypeId != (int)FileType.Supplementary)
                {
                    QueryableConfig qd = new QueryableConfig();
                    qd.configName = schemaDto.Name;
                    qd.bucket = RootBucket;

                    qd.s3Key = schemaDto.RawQueryStorage;

                    List<DatasetFile> dfList = _schemaService.GetDatasetFilesBySchema(schemaDto.SchemaId).ToList();

                    qd.fileCount = dfList.Count;
                    DatasetFile latestFile = _schemaService.GetLatestDatasetFileBySchema(schemaDto.SchemaId);
                    qd.primaryFileId = (latestFile != null) ? latestFile.DatasetFileId.ToString() : null;
                    qd.extensions = dfList.Select(x => Utilities.GetFileExtension(x.FileName)).Distinct().ToList();
                    qd.description = schemaDto.Description;

                    List<SchemaRevisionDto> schemaRevDtoList = _schemaService.GetSchemaRevisionDtoBySchema(schemaDto.SchemaId);
                    qd.HasSchema = (schemaRevDtoList.Any());
                    qd.HasQueryableSchema = qd.HasSchema;
                    
                    List<QueryableSchema> qslist = new List<QueryableSchema>();
                    if (qd.HasSchema)
                    {
                        //only take the latest revision for now.  Need to revist if support for querying multiple revisions is needed
                        foreach (var sch in schemaRevDtoList.OrderByDescending(o => o.CreatedDTM).Take(1))
                        {
                            QueryableSchema qs = new QueryableSchema()
                            {
                                SchemaName = schemaDto.Name,
                                SchemaDSC = schemaDto.Description,
                                SchemaID = schemaDto.SchemaId,
                                RevisionID = sch.RevisionId
                            };

                            //This is assuming only a single hive table per schema revision.
                            // Checking status to ensure table is ready for querying.
                            if (schemaDto.HiveTable != null && 
                                (schemaDto.HiveStatus == ConsumptionLayerTableStatusEnum.Pending.ToString() || 
                                 schemaDto.HiveStatus == ConsumptionLayerTableStatusEnum.Requested.ToString() || 
                                 schemaDto.HiveStatus == ConsumptionLayerTableStatusEnum.Available.ToString())
                                 )
                            {
                                qs.HiveDatabase = schemaDto.HiveDatabase;
                                qs.HiveTable = schemaDto.HiveTable;
                                qs.HiveTableStatus = schemaDto.HiveStatus;
                                qs.HasTable = true;
                            }
                            else
                            {
                                qs.HasTable = false;
                            }
                            qslist.Add(qs);
                        }
                    }
                    qd.Schemas = qslist;                    
                    reply.Add(qd);
                }
            }

            QueryableDataset output = new QueryableDataset() { Configs = reply };

            output.Security = us;
            output.datasetCategory = ds.DatasetCategories.First().Name;
            output.datasetColor = ds.DatasetCategories.First().Color;

            return Ok(output);


            //foreach (var item in ds.DatasetFileConfigs.Where(w => w.DeleteInd == false))
            //{
            //    if (item.FileTypeId != (int)FileType.Supplementary)
            //    {
            //        QueryableConfig qd = new QueryableConfig();

            //        qd.configName = item.Name;
            //        qd.bucket = RootBucket;
            //        qd.s3Key = Utilities.GenerateLocationKey(item);


            //        qd.fileCount = ds.DatasetFiles.Where(x => x.DatasetFileConfig.ConfigId == item.ConfigId && x.ParentDatasetFileId == null).ToList().Count;

            //        if (ds.DatasetFiles.OrderBy(x => x.CreateDTM).FirstOrDefault(x => x.DatasetFileConfig.ConfigId == item.ConfigId) != null)
            //        {
            //            qd.primaryFileId = ds.DatasetFiles.OrderBy(x => x.CreateDTM).FirstOrDefault(x => x.DatasetFileConfig.ConfigId == item.ConfigId).DatasetFileId.ToString();
            //        }
            //        qd.extensions = ds.DatasetFiles.Where(x => x.DatasetFileConfig.ConfigId == item.ConfigId).Select(x => Utilities.GetFileExtension(x.FileName)).Distinct().ToList();
            //        qd.description = item.Description;
            //        qd.HasSchema = item.Schemas.FirstOrDefault().DataObjects.Any();

            //        qd.HasQueryableSchema = item.Schemas.FirstOrDefault().DataObjects.Any();

            //        if (qd.HasSchema)
            //        {
            //            List<QueryableSchema> qslist = new List<QueryableSchema>();
            //            foreach (var sch in item.Schemas)
            //            {
            //                QueryableSchema qs = new QueryableSchema()
            //                {
            //                    SchemaName = sch.SchemaName,
            //                    SchemaDSC = sch.SchemaDescription,
            //                    SchemaID = sch.DataElement_ID,
            //                    RevisionID = sch.SchemaRevision
            //                };

            //                //This is assuming only a single hive table per schema revision.
            //                // Checking status to ensure table is ready for querying.
            //                if (sch.HiveTable != null)
            //                {
            //                    qs.HiveDatabase = sch.HiveDatabase;
            //                    qs.HiveTable = sch.HiveTable;
            //                    qs.HiveTableStatus = sch.HiveTableStatus;
            //                    qs.HasTable = true;
            //                }
            //                else
            //                {
            //                    qs.HasTable = false;
            //                }
            //                qslist.Add(qs);
            //            }
            //            qd.Schemas = qslist;
            //        }
            //        reply.Add(qd);
            //    }
            //}

            //QueryableDataset output = new QueryableDataset() { Configs = reply };

            //output.datasetCategory = ds.DatasetCategories.First().Name;
            //output.datasetColor = ds.DatasetCategories.First().Color;

            //return Ok(output);
        }


        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("dataset/{datasetId}/config/{configId}/SampleRecords")]
        public async Task<IHttpActionResult> GetSampleRecords(int datasetId, int configId, int rows)
        {
            Logger.AddContextVariable(new TextVariable("requestcontextguid", DateTime.UtcNow.ToString(GlobalConstants.System.REQUEST_CONTEXT_GUID_FORMAT)));
            Logger.AddContextVariable(new TextVariable("requestcontextdatasetid", datasetId.ToString()));
            Logger.AddContextVariable(new TextVariable("requestcontextconfigid", configId.ToString()));
            Logger.AddContextVariable(new TextVariable("requestcontextuserid", _userService.GetCurrentUser().AssociateId));


            UserSecurity us = _datasetService.GetUserSecurityForDataset(datasetId);
            
            if (!us.CanViewData)
            {
                return Content(System.Net.HttpStatusCode.Unauthorized, "Unauthroized Access to Dataset");
            }

            try
            {
                List<Dictionary<string, object>> results = _schemaService.GetTopNRowsByConfig(configId, rows);

                string sJSON = JsonConvert.SerializeObject(results);
                return Ok(sJSON);
            }
            catch (HiveTableViewNotFoundException)
            {
                return Content(System.Net.HttpStatusCode.NotFound, "Table or view not found");
            }
            catch (SchemaNotFoundException snfex)
            {
                return Content(System.Net.HttpStatusCode.NotFound, snfex.Message);
            }

            //System.Data.DataTable _dt = new System.Data.DataTable();

            //_dt.Columns.Add("ID");
            //_dt.Columns.Add("Name");

            //System.Data.DataRow dr1 = _dt.NewRow();
            //dr1["ID"] = 1;
            //dr1["Name"] = "Smruti";
            //_dt.Rows.Add(dr1);

            //System.Data.DataRow dr2 = _dt.NewRow();
            //dr2["ID"] = 2;
            //dr2["Name"] = "Ranjan";
            //_dt.Rows.Add(dr2);


            //List<Dictionary<string, object>> dicRows = new List<Dictionary<string, object>>();
            //Dictionary<string, object> dicRow = null;
            //foreach (System.Data.DataRow dr in _dt.Rows)
            //{
            //    dicRow = new Dictionary<string, object>();
            //    foreach (System.Data.DataColumn col in _dt.Columns)
            //    {
            //        dicRow.Add(col.ColumnName, dr[col]);
            //    }
            //    dicRows.Add(dicRow);
            //}

            //string sJSON = JsonConvert.SerializeObject(dicRows);
            //return Ok(sJSON);
        }

        private async Task<LivyReply> WaitForLivyReply(int SessionID, int StatementID)
        {
            LivyReply lr = new LivyReply();

            while (lr.state != "available")
            {
                IHttpActionResult response = await GetStatement(SessionID, StatementID).ConfigureAwait(false);

                if (response.GetType() == typeof(OkNegotiatedContentResult<String>))
                {
                    var a = response as OkNegotiatedContentResult<String>;
                    lr = JsonConvert.DeserializeObject<LivyReply>(a.Content);
                }
                else
                {
                    //Something happened and Livy Broke.  :(
                    return null;
                }

                //Wait before polling again
                Thread.Sleep(500);
            }


            return lr;
        }

        //[HttpGet]
        ////[Route("Get")]
        //[AuthorizeByPermission(PermissionNames.QueryToolUser)]
        //public async Task<IHttpActionResult> AddFileToHiveTable(int SessionID, string s3Key, int configID)
        //{
        //    //Make sure that everything on that config is loaded into the Metadata Repository
        //    IHttpActionResult stepOne = await (CreateDataElementandObject(configID));

        //    if (stepOne.GetType() != typeof(OkResult))
        //    {
        //        return BadRequest(stepOne.ToString());
        //    }

        //    //Check the schema of the file against the metadata repository
        //    IHttpActionResult stepTwo = await (CheckSchema(SessionID, s3Key, configID));

        //    if (stepTwo.GetType() != typeof(OkNegotiatedContentResult<String>))
        //    {
        //        var result = stepTwo as BadRequestErrorMessageResult;
        //        return BadRequest(result.Message);
        //    }

        //    var a = stepTwo as OkNegotiatedContentResult<String>;
        //    var guid = a.Content;

        //    //Check to see if a Hive Table Exists for a given Config
        //    IHttpActionResult stepThree = await (CreateHiveTable(SessionID, configID));

        //    if (stepThree.GetType() != typeof(OkNegotiatedContentResult<String>))
        //    {
        //        return BadRequest(stepThree.ToString());
        //    }

        //    var b = stepThree as OkNegotiatedContentResult<String>;
        //    var hiveTableName = b.Content;

        //    IHttpActionResult stepFour = await (CreateParquet(SessionID, guid, configID, true));

        //    if (stepFour.GetType() != typeof(OkNegotiatedContentResult<String>))
        //    {
        //        return BadRequest(stepFour.ToString());
        //    }

        //    var c = stepFour as OkNegotiatedContentResult<String>;
        //    var dropLocation = c.Content;

        //    return Ok("Hive Table: " + hiveTableName + " - Drop Location: " + dropLocation);
        //}


        ///// <summary>
        ///// S3KEY COULD BE A FILE OR STAR PREFIX IN S3.
        ///// We need the Session ID to identify where that DataFrame is and the ConfigID to know where the Hive table is located and its Schema in the Metadata Repository.
        ///// </summary>
        ///// <param name="SessionID"></param>
        ///// <param name="s3Key"></param>
        ///// <param name="configID"></param>
        ///// <returns></returns>
        //[HttpGet][Route("get")]
        ////[Route("Get")]
        //[AuthorizeByPermission(PermissionNames.QueryToolUser)]
        //public async Task<IHttpActionResult> CheckSchema(int SessionID, string s3Key, int configID)
        //{
        //    Guid guid = Guid.NewGuid();

        //    try
        //    {                
        //        IHttpActionResult response = await (SendCode(SessionID, _livy.GetDataFrameFromS3Key(guid, s3Key, configID)));

        //        if (response.GetType() == typeof(OkNegotiatedContentResult<String>))
        //        {
        //            //We can save this Schema back to the Database easily now that we have it.
        //            var a = response as OkNegotiatedContentResult<String>;
        //            LivyReply lr = JsonConvert.DeserializeObject<LivyReply>(a.Content);

        //            lr = await WaitForLivyReply(SessionID, lr.id);

        //            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configID);
        //            var dataObjectID = _datasetContext.Schemas.Where(x => x.DatasetFileConfig.ConfigId == dfc.ConfigId).FirstOrDefault().DataObject_ID;
        //            DataObject dataObject = _datasetContext.GetById<DataObject>(dataObjectID);

        //            String output = "";
        //            if (dataObject.DataObjectFields.Any())
        //            {
        //                output = _livy.CompareSchemas(_livy.GetHiveColumns(lr), dataObject.DataObjectFields);
        //            }
        //            else
        //            {
        //                CreateDataObjectFields(configID, _livy.GetHiveColumns(lr));
        //            }

        //            if (output == "")
        //            {
        //                //If schema is the same happily give back the Ok and Guid so that the parquet can be created.
        //                return Ok("tmp_" + guid.ToString("N"));
        //            }
        //            else
        //            {
        //                //If the schema is not the same there is a lot of work that needs to be done.
        //                return BadRequest(output);
        //            }
        //        }
        //        else
        //        {
        //            return response;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError();
        //    }
        //}

        /// <summary>
        /// gets a hive table
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="configID"></param>
        /// <param name="rows"></param>
        /// <param name="skip"></param>
        /// <param name="hiveTableName"></param>
        /// <param name="hiveDatabaseName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("sessions/{SessionID}/hives/{hiveDatabaseName}/{hiveTableName}/{configID}/{rows}/{skip}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> GetHiveTable(int SessionID, int configID, int rows, int skip, string hiveTableName, string hiveDatabaseName)
        {
            String python;
            
            if (hiveDatabaseName != null)
            {
                python = $"spark.sql('SELECT * FROM {hiveDatabaseName}.{hiveTableName}').show({rows}, False)";
            }
            else
            {
                python = $"spark.sql('SELECT * FROM {hiveTableName}').show({rows}, True)";
            }            

            String quoted = System.Web.Helpers.Json.Encode(python);
            quoted = quoted.Substring(1, quoted.Length - 2);

            IHttpActionResult response = await SendCode(SessionID, quoted).ConfigureAwait(false);

            if (response.GetType() == typeof(OkNegotiatedContentResult<String>))
            {
                //We can save this Schema back to the Database easily now that we have it.
                var a = response as OkNegotiatedContentResult<String>;
                LivyReply lr = JsonConvert.DeserializeObject<LivyReply>(a.Content);

                lr = await WaitForLivyReply(SessionID, lr.id).ConfigureAwait(false);

                return Ok(lr.output.data.text);
            }
            else
            {
                return response;
            }
        }


        /// <summary>
        /// saves a row count to a session
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="guid"></param>
        /// <param name="configID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sessions/{SessionID}/hives/{configID}/{guid}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> SaveRowCount(int SessionID, String guid, int configID)
        {
            return StatusCode(HttpStatusCode.NoContent);
            //String python;

            //python = guid + ".count()";

            //String quoted = System.Web.Helpers.Json.Encode(python);
            //quoted = quoted.Substring(1, quoted.Length - 2);

            //IHttpActionResult response = await (SendCode(SessionID, quoted));

            //if (response.GetType() == typeof(OkNegotiatedContentResult<String>))
            //{
            //    //We can save this Schema back to the Database easily now that we have it.
            //    var a = response as OkNegotiatedContentResult<String>;
            //    LivyReply lr = JsonConvert.DeserializeObject<LivyReply>(a.Content);

            //    lr = await WaitForLivyReply(SessionID, lr.id);

            //    DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configID);
            //    var dataObjectID = _datasetContext.Schemas.Where(x => x.DatasetFileConfig.ConfigId == dfc.ConfigId).FirstOrDefault().DataObject_ID;
            //    DataObject dataObject = _datasetContext.GetById<DataObject>(dataObjectID);

            //    if (dataObject.DataObjectDetails.Any(x => x.DataObjectDetailType_CDE == "Row_CNT"))
            //    {
            //        var detail = dataObject.DataObjectDetails.FirstOrDefault(x => x.DataObjectDetailType_CDE == "Row_CNT");
            //        detail.DataObjectDetailType_VAL = (Convert.ToInt32(detail.DataObjectDetailType_VAL) + Convert.ToInt32(lr.output.data.text)).ToString();

            //        _datasetContext.Merge<DataObjectDetail>(detail);
            //        _datasetContext.SaveChanges();
            //    }
            //    else
            //    {
            //        dataObject.DataObjectDetails.Add(
            //            new DataObjectDetail()
            //            {
            //                DataObject = dataObject,
            //                DataObjectDetailType_CDE = "Row_CNT",
            //                DataObjectDetailType_VAL = lr.output.data.text,
            //                DataObjectDetailCreate_DTM = DateTime.Now,
            //                DataObjectDetailChange_DTM = DateTime.Now,
            //                LastUpdt_DTM = DateTime.Now
            //            });
            //        _datasetContext.Merge<DataObject>(dataObject);
            //        _datasetContext.SaveChanges();
            //    }

            //    return Ok(lr.output.data.text);
            //}
            //else
            //{
            //    return response;
            //}


        }

        /// <summary>
        /// THIS METHOD WILL ONLY WORK IF THE CHECK SCHEMA METHOD WAS SUCCESSFUL. 
        /// THIS METHOD REQUIRES THE GUID GENERATED FROM THAT METHOD.
        /// The GUID itself is the pointer in the Temporary DataFrame in Memory in Hadoop/Spark.
        /// We need the Session ID to identify where that DataFrame is and the ConfigID to know where the Hive table is located.
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="guid"></param>
        /// <param name="configID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sessions/{SessionID}/hives/{hiveTableExists}/{configID}/{guid}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> CreateParquet(int SessionID, String guid, int configID, Boolean hiveTableExists)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configID);

            String python;
            String tableName = dfc.ParentDataset.DatasetId + "_" + dfc.ConfigId + "_" + dfc.Name.Replace(' ', '_').Replace('-', '_');
            String dropLocation = "";

            //Get Write Mode Strategy
            String writeMode = "append";

            if (hiveTableExists)
            {
                //It requires that the schema of the class:DataFrame is the same as the schema of the table.

                python = guid + @".write";
                python += ".mode('" + writeMode + "')";
                python += ".insertInto('" + tableName + "')";

                dropLocation = tableName;
            }
            else
            {
                python = guid + @".write.format('parquet')";

                python += ".mode('" + writeMode + "')";

                //Get Partitioning Strategy
                //STATIC PARTITIONING
                //adding partition statically and loading data into it,takes less time than dynamic partitions as it won't need to look into data while creating partitions.

                //DYNAMIC PARTITIONING
                //creating partitions dynamically based on the column value, take more time than static partitions if data is huge because it needs to look into data while creating partitions.

                //python += ".partitionBy('own_code')";

                String bucket = RootBucket;
                String s3Prefix = Config.GetHostSetting("S3DataPrefix");

                dropLocation = "s3a://"
                    + bucket + "/"
                    + s3Prefix + "/"
                    + "parquet" + "/"
                    + dfc.ParentDataset.DatasetCategories.First().Id + "/"
                    + dfc.ParentDataset.DatasetId + "/"
                    + dfc.ConfigId + "/"
                    //Schema Revision
                    ;

                python += ".save('" + dropLocation + "')";
            }


            String quoted = System.Web.Helpers.Json.Encode(python);
            quoted = quoted.Substring(1, quoted.Length - 2);

            IHttpActionResult response = await SendCode(SessionID, quoted).ConfigureAwait(false);

            if (response.GetType() == typeof(OkNegotiatedContentResult<String>))
            {
                //We can save this Schema back to the Database easily now that we have it.
                var a = response as OkNegotiatedContentResult<String>;
                LivyReply lr = JsonConvert.DeserializeObject<LivyReply>(a.Content);

                lr = await WaitForLivyReply(SessionID, lr.id).ConfigureAwait(false);

                return Ok(dropLocation);
            }
            else
            {
                return response;
            }
        }

        /// <summary>
        /// CREATES A HIVE TABLE IF IT DOESNT ALREADY EXIST. ELSE NOTHING HAPPENS.
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="configID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sessions/{SessionID}/hives/{configID}")]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        public async Task<IHttpActionResult> CreateHiveTable(int SessionID, int configID)
        {
            return StatusCode(HttpStatusCode.NoContent);
            //DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configID);

            //String bucket = RootBucket;
            //String s3Prefix = Sentry.Configuration.Config.GetHostSetting("S3DataPrefix");

            //String dropLocation = "s3a://"
            //    + bucket + "/"
            //    + s3Prefix + "/"
            //    + "parquet" + "/"
            //    + dfc.ParentDataset.DatasetCategory.Id + "/"
            //    + dfc.ParentDataset.DatasetId + "/"
            //    + dfc.ConfigId + "/"
            //    //Schema Revision
            //    ;

            ////Yes Python -> Livy -> Spark -> Hive seriously needs triple quotes.

            //String python = @"spark.sql(""""""CREATE TABLE IF NOT EXISTS";

            //String hiveTableName = dfc.ParentDataset.DatasetId + "_" + dfc.ConfigId + "_" + dfc.Name.Replace(' ', '_').Replace('-', '_');

            //python += "`" + hiveTableName + "`";

            //var dataObjectID = _datasetContext.Schemas.Where(x => x.DatasetFileConfig.ConfigId == dfc.ConfigId).FirstOrDefault().DataObject_ID;
            //DataObject dataObject = _datasetContext.GetById<DataObject>(dataObjectID);

            //if (dataObject != null)
            //{
            //    python += " (";

            //    foreach (DataObjectField b in dataObject.DataObjectFields)
            //    {
            //        String sqlServerDataType = null;
            //        if (b.DataObjectFieldDetails.Any(x => x.DataObjectFieldDetailType_CDE == "Datatype_TYP"))
            //        {
            //            sqlServerDataType = LivyHelper.SQLServerDataTypeToHive(b.DataObjectFieldDetails.FirstOrDefault(x => x.DataObjectFieldDetailType_CDE == "Datatype_TYP").DataObjectFieldDetailType_VAL);
            //        }
            //        else
            //        {
            //            sqlServerDataType = LivyHelper.SQLServerDataTypeToHive("NVARCHAR");
            //        }

            //        python += $"`{b.DataObjectField_NME}` {sqlServerDataType},";

            //    }

            //    python = python.TrimEnd(',');
            //    python += ")";

            //    //IT WILL LOOK LIKE THIS
            //    // "(`area_fips` string,`own_code` int,`industry_code` string,`agglvl_code` int,`size_code` int,`year` int)"
            //}
            //else
            //{
            //    //THERE IS NOTHING IN THE METADATA REPOSITORY FOR THIS FILE.
            //}
            ////PARTITIONED BY (THING DATATYPE)

            //python += "STORED AS PARQUET LOCATION '" + dropLocation + @"'"""""")";




            //String quoted = System.Web.Helpers.Json.Encode(python);
            //quoted = quoted.Substring(1, quoted.Length - 2);

            //IHttpActionResult response = await (SendCode(SessionID, quoted));
            //var type = response.GetType();

            //if (type == typeof(OkResult))
            //{
            //    return Ok(hiveTableName);
            //}
            //else
            //{
            //    return response;
            //}
        }

        //[HttpGet][Route("get")]
        ////[Route("Get")]
        //[AuthorizeByPermission(PermissionNames.QueryToolUser)]
        //public async Task<IHttpActionResult> CreateDataElementandObject(int configID)
        //{
        //    /*  DATA ELEMENT CREATION:
        //    *    THIS CORRELATES TO A DATASET
        //    */
        //    DataElement de = null;
        //    try
        //    {
        //        DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configID);

        //        if (dfc.DataElement != null)
        //        {
        //            de = dfc.DataElement;
        //        }
        //        else
        //        {
        //            de = new DataElement()
        //            {
        //                MetadataAsset = null,
        //                DataElement_NME = dfc.Name,
        //                DataElement_DSC = dfc.Description,
        //                DataElement_CDE = "F",
        //                DataElementCode_DSC = "Data File",
        //                DataElementCreate_DTM = DateTime.Now,
        //                DataElementChange_DTM = DateTime.Now,
        //                LastUpdt_DTM = DateTime.Now
        //            };

        //            _datasetContext.Add<DataElement>(de);
        //            _datasetContext.SaveChanges();
        //            de = _datasetContext.DataElements.FirstOrDefault(x => x.DataElement_NME == dfc.Name);

        //            dfc.DataElement = de;
        //            _datasetContext.Merge<DatasetFileConfig>(dfc);
        //            _datasetContext.SaveChanges();


        //            /*  DATA ELEMENT DETAIL CREATION:
        //             *   THESE ARE FIELDS THAT COULD BE STORED ON THE CONFIG
        //             */

        //            de.DataElementDetails = new List<DataElementDetail>();

        //            de.DataElementDetails.Add(new DataElementDetail()
        //            {
        //                DataElement = de,
        //                DataElementDetailType_CDE = "FileFormat_TYP",
        //                DataElementDetailType_VAL = "json",// ALREADY STORED ON THE CONFIG
        //                DataElementDetailCreate_DTM = DateTime.Now,
        //                DataElementDetailChange_DTM = DateTime.Now,
        //                LastUpdt_DTM = DateTime.Now
        //            });

        //            de.DataElementDetails.Add(new DataElementDetail()
        //            {
        //                DataElement = de,
        //                DataElementDetailType_CDE = "FileDelimiter_TYP",
        //                DataElementDetailType_VAL = ",",
        //                DataElementDetailCreate_DTM = DateTime.Now,
        //                DataElementDetailChange_DTM = DateTime.Now,
        //                LastUpdt_DTM = DateTime.Now
        //            });

        //            _datasetContext.Merge<DataElement>(de);
        //            _datasetContext.SaveChanges();

        //        }
        //    }catch(Exception ex)
        //    {
        //        return BadRequest();
        //    }


        //    /*  DATA OBJECT CREATION:
        //    *    ESSENTIALLY THIS IS A SCHEMA.  THIS CORRELATES TO A DATA FILE CONFIG
        //    */
        //    try
        //    {
        //        DatasetFileConfig dfc2 = _datasetContext.GetById<DatasetFileConfig>(configID);

        //        Schema schema = dfc2.Schemas.OrderByDescending(x => x.Revision_ID).FirstOrDefault();

        //        DataObject dataObject;

        //        if (schema.DataObject_ID != 0)
        //        {
        //            dataObject = _datasetContext.GetById<DataObject>(schema.DataObject_ID);
        //        }
        //        else
        //        {
        //            dataObject = new DataObject()
        //            {
        //                DataElement = de,
        //                DataElement_ID = de.DataElement_ID,
        //                DataObject_NME = schema.Schema_NME,
        //                DataObject_DSC = schema.Schema_DSC,
        //                DataObject_CDE = "C",  //Duplicate of Data Element Detail.
        //                DataObjectCode_DSC = "CSV File",  //Duplicate of Data Element Detail. ALREADY STORED ON THE CONFIG.
        //                DataObjectCreate_DTM = DateTime.Now,
        //                DataObjectChange_DTM = DateTime.Now,
        //                LastUpdt_DTM = DateTime.Now
        //            };

        //            _datasetContext.Add<DataObject>(dataObject);
        //            _datasetContext.SaveChanges();
        //            dataObject = _datasetContext.DataObjects.FirstOrDefault(x => x.DataElement == de && x.DataObject_NME == schema.Schema_NME);

        //            //Cast the ID onto the Dataset File Config           
        //            schema.DataObject_ID = dataObject.DataObject_ID;
        //            _datasetContext.Merge<Schema>(schema);
        //            _datasetContext.SaveChanges();


        //            /*  DATA OBJECT DETAIL CREATION:
        //             *   THESE ARE FIELDS THAT COULD BE STORED ON THE CONFIG
        //             */

        //            List<DataObjectDetail> dataObjectDetails = new List<DataObjectDetail>();

        //            DataObjectDetail dod = new DataObjectDetail()
        //            {
        //                DataObject = dataObject,
        //                DataObjectDetailType_CDE = "HeaderRow_IND",
        //                DataObjectDetailType_VAL = "Y",
        //                DataObjectDetailCreate_DTM = DateTime.Now,
        //                DataObjectDetailChange_DTM = DateTime.Now,
        //                LastUpdt_DTM = DateTime.Now
        //            };
        //            dataObjectDetails.Add(dod);

        //            dataObject.DataObjectDetails = dataObjectDetails;

        //            _datasetContext.Add<DataObject>(dataObject);
        //            _datasetContext.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest();
        //    }

        //    return Ok();
        //}

        //[AuthorizeByPermission(PermissionNames.QueryToolUser)]
        //private void CreateDataObjectFields(int configID, List<HiveColumn> hiveColumns)
        //{
        //    DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configID);
        //    var dataObjectID = _datasetContext.Schemas.Where(x => x.DatasetFileConfig.ConfigId == dfc.ConfigId).FirstOrDefault().DataObject_ID;
        //    DataObject dataObject = _datasetContext.GetById<DataObject>(dataObjectID);

        //    /*  DATA OBJECT FIELDS CREATION:
        //    *    THESE ARE THE COLUMNS IN THE SCHEMA / DATA OBJECT
        //    */

        //    if (dataObject != null)
        //    {
        //        foreach (HiveColumn hc in hiveColumns)
        //        {


        //            DataObjectField dof = new DataObjectField()
        //            {
        //                DataObject = dataObject,
        //                DataObjectField_NME = hc.name,
        //                DataObjectFieldCreate_DTM = DateTime.Now,
        //                DataObjectFieldChange_DTM = DateTime.Now,
        //                LastUpdt_DTM = DateTime.Now
        //            };

        //            dof.DataObjectFieldDetails = new List<DataObjectFieldDetail>();

        //            dof.DataObjectFieldDetails.Add(new DataObjectFieldDetail()
        //            {
        //                DataObjectField = dof,
        //                DataObjectFieldDetailType_VAL = hc.datatype,
        //                DataObjectFieldDetailType_CDE = "Datatype_TYP",
        //                DataObjectFieldDetailCreate_DTM = DateTime.Now,
        //                DataObjectFieldDetailChange_DTM = DateTime.Now,
        //                LastUpdt_DTM = DateTime.Now
        //            });

        //            dof.DataObjectFieldDetails.Add(new DataObjectFieldDetail()
        //            {
        //                DataObjectField = dof,
        //                DataObjectFieldDetailType_VAL = hc.nullable == true ? "Y" : "N",
        //                DataObjectFieldDetailType_CDE = "Nullable_IND",
        //                DataObjectFieldDetailCreate_DTM = DateTime.Now,
        //                DataObjectFieldDetailChange_DTM = DateTime.Now,
        //                LastUpdt_DTM = DateTime.Now
        //            });

        //            dataObject.DataObjectFields.Add(dof);

        //            _datasetContext.Add<DataObjectField>(dof);
        //            _datasetContext.SaveChanges();

        //        }
        //    }
        //}
    }
}

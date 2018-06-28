using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Sentry.data.Infrastructure;
using Sentry.Configuration;
using System.Web.Http.Results;
using Sentry.data.Common;
using Amazon.S3.Model;

namespace Sentry.data.Web.Controllers
{
    public class QueryToolController : ApiController
    {
        public IAssociateInfoProvider _associateInfoProvider;
        public IDatasetContext _datasetContext;
        private UserService _userService;
        private S3ServiceProvider _s3Service;
        public string _livyUrl;

        public QueryToolController(IDatasetContext dsCtxt, S3ServiceProvider dsSvc, UserService userService, ISASService sasService, IAssociateInfoProvider associateInfoService)
        {
            _datasetContext = dsCtxt;
            _userService = userService;
            _s3Service = dsSvc;
            _associateInfoProvider = associateInfoService;
            _livyUrl = Sentry.Configuration.Config.GetHostSetting("ApacheLivy");
        }

        [HttpPost]
        [Route("Create")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> CreateSession(string Language)
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {
                string json;
                switch (Language)
                {
                    default:
                    case "Python":
                        json = "{\"kind\": \"pyspark\"}";
                        break;
                    case "Scala":
                        json = "{\"kind\": \"spark\"}";
                        break;
                    case "R":
                        json = "{\"kind\": \"rspark\"}";
                        break;
                }

                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                HttpResponseMessage response = await client.PostAsync(_livyUrl  + "/sessions", contentPost);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(response.Content.ReadAsStringAsync().Result);
                }
                else if(response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest();
                }
                else
                {
                    return NotFound();
                }
            }
        }

        [HttpPost]
        [Route("Send")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> SendCode(int SessionID, [FromBody] string Code)
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {

                //dynamic json = Code;
                var json = "{\"code\": \"" + Code + "\"}";

                //json = new JavaScriptSerializer().Serialize();

                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                HttpResponseMessage response = await client.PostAsync(_livyUrl + "/sessions/" + SessionID + "/statements", contentPost);

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
        }



        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetSessions()
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.GetAsync(_livyUrl + "/sessions");

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
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetSession(int SessionID)
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.GetAsync(_livyUrl + "/sessions/" + SessionID);

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
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetStatement(int SessionID, int StatementID)
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.GetAsync(_livyUrl + "/sessions/" + SessionID + "/statements/" + StatementID);
                
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
        }

        [HttpPost]
        [Route("Post")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> CancelStatement(int SessionID, int StatementID)
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.PostAsync(_livyUrl + "/sessions/" + SessionID + "/statements/" + StatementID + "/cancel", null);

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
        }

        [HttpDelete]
        [Route("Delete")]
        [AuthorizeByPermission(PermissionNames.QueryToolAdmin)]
        public async Task<IHttpActionResult> DeleteSession(int SessionID)
        {
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.DeleteAsync(_livyUrl + "/sessions/" + SessionID);

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
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> FileDropLocation()
        {
            IApplicationUser user = _userService.GetCurrentUser();

            var obj = new
            {
                s3Key = Sentry.Configuration.Config.GetHostSetting("AWSRootBucket") + "/" + Utilities.GenerateDatasetStorageLocation("QueryTool/Bundle", user.AssociateId)
            };

            return Ok(obj);
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
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

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetDatasetFileDownloadURL(string s3Key)
        {
            try
            {
                List<string> list = _s3Service.FindObject(s3Key);

                foreach(string obj in list)
                {
                    if(obj.Contains("part-00000"))
                    {
                        return Ok(_s3Service.GetDatasetDownloadURL(obj));
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        private class QueryableDataset
        {
            public List<QueryableConfig> Configs { get; set; }
            public string datasetCategory { get; set; }
            public string datasetColor { get; set; }
        }

        private class QueryableConfig
        {
            public string configName { get; set; }
            public string bucket { get; set; }
            public string s3Key { get; set; }
            public string description { get; set; }

            public string primaryFileId { get; set; }

            public List<string> extensions { get; set; }
            public int fileCount { get; set; }
            public Boolean IsGeneric { get; set; }
            public Boolean IsPowerUser { get; set; }
        }

        [HttpGet]
        [Route("Get")]
        [AuthorizeByPermission(PermissionNames.QueryToolUser)]
        public async Task<IHttpActionResult> GetS3Key(int datasetID)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(datasetID);

            List<QueryableConfig> reply = new List<QueryableConfig>();

            foreach (var item in ds.DatasetFileConfigs)
            {
                if (item.FileTypeId != (int) FileType.Supplementary)
                {
                    QueryableConfig qd = new QueryableConfig();

                    qd.configName = item.Name;
                    qd.bucket = Sentry.Configuration.Config.GetHostSetting("AWSRootBucket");
                    qd.s3Key = ds.S3Key + item.ConfigId;
                    

                    qd.fileCount = ds.DatasetFiles.Where(x => x.DatasetFileConfig.ConfigId == item.ConfigId && x.ParentDatasetFileId == null).ToList().Count;

                    if (ds.DatasetFiles.OrderBy(x => x.CreateDTM).FirstOrDefault(x => x.DatasetFileConfig.ConfigId == item.ConfigId) != null)
                    {
                        qd.primaryFileId = ds.DatasetFiles.OrderBy(x => x.CreateDTM).FirstOrDefault(x => x.DatasetFileConfig.ConfigId == item.ConfigId).DatasetFileId.ToString();
                    }
                    qd.extensions = ds.DatasetFiles.Where(x => x.DatasetFileConfig.ConfigId == item.ConfigId).Select(x => Utilities.GetFileExtension(x.FileName)).Distinct().ToList();
                    qd.description = item.Description;                   

                    qd.IsGeneric = item.IsGeneric;
                    qd.IsPowerUser = _userService.GetCurrentUser().CanQueryToolPowerUser;

                    reply.Add(qd);
                }
            }

            QueryableDataset output = new QueryableDataset() { Configs = reply };

            output.datasetCategory = ds.DatasetCategory.Name;
            output.datasetColor = ds.DatasetCategory.Color;

            return Ok(output);
        }
    }
}

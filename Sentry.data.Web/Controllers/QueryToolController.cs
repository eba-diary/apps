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

namespace Sentry.data.Web.Controllers
{
    public class QueryToolController : ApiController
    {
        public IDatasetContext _datasetContext;
        public string _livyUrl;

        public QueryToolController(IDatasetContext dsCtxt)
        {
            _datasetContext = dsCtxt;
            _livyUrl = Sentry.Configuration.Config.GetHostSetting("ApacheLivy");
        }

        [HttpPost]
        [Route("Create")]
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
    }
}

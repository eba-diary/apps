using Sentry.WebAPI.Versioning;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sentry.data.Web.WebApi.Controllers;
using System.Threading.Tasks;
using Swashbuckle.Swagger.Annotations;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_DATAFLOW)]
    public class DataFlowController : BaseWebApiController
    {
        private readonly IDataFlowService _dataFlowService;

        public DataFlowController(IDataFlowService dataFlowservice)
        {
            _dataFlowService = dataFlowservice;
        }

        private IDataFlowService DataFlowService
        {
            get { return _dataFlowService; }
        }

        ////// GET api/<controller>
        ////public IEnumerable<string> Get()
        ////{
        ////    return new string[] { "value1", "value2" };
        ////}

        ////// GET api/<controller>/5
        ////public string Get(int id)
        ////{
        ////    return "value";
        ////}

        ////// POST api/<controller>
        ////public void Post([FromBody] string value)
        ////{
        ////}

        ////// PUT api/<controller>/5
        ////public void Put(int id, [FromBody] string value)
        ////{
        ////}

        ////// DELETE api/<controller>/5
        ////public void Delete(int id)
        ////{
        ////}

        /// <summary>
        /// Create new dataflow (v3) for each dataflow provided
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        [ApiVersionBegin(WebAPI.Version.v2)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("updatedataflows")]
        [HttpPost]
        public IHttpActionResult UpdateDataFlows(int[] idList)
        {
            DataFlowService.UpgradeDataFlows(idList);

            return Ok();
        }

        /// <summary>
        /// Delete each dataflow associated with id(s) provided
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        [ApiVersionBegin(WebAPI.Version.v2)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("deletedataflows")]
        [HttpPost]
        public IHttpActionResult DeleteDataFlows(int[] idList)
        {
            
            DataFlowService.DeleteDataFlows(idList);

            return Ok();
        }
    }
}
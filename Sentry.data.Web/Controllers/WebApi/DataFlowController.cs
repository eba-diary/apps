using Sentry.data.Core;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_DATAFLOW)]
    public class DataFlowController : BaseWebApiController
    {
        private readonly IDataFlowService _dataFlowService;
        private readonly IUserService _userService;
        private readonly Lazy<IDataApplicationService> _dataApplicationService;

        public DataFlowController(IDataFlowService dataFlowservice, Lazy<IDataApplicationService> dataApplicationService,
            IUserService userService)
        {
            _dataFlowService = dataFlowservice;
            _dataApplicationService = dataApplicationService;
            _userService = userService;
        }

        private IDataFlowService DataFlowService
        {
            get { return _dataFlowService; }
        }

        private IDataApplicationService DataApplicaitonService
        {
            get { return _dataApplicationService.Value; }
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

            bool isSuccessful = DataApplicaitonService.DeleteDataFlow_Queue(idList.ToList(), _userService.GetCurrentUser().AssociateId, true);

            if (!isSuccessful)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, "Unable to queue all deletes");
            }
            return Ok();
        }
    }
}
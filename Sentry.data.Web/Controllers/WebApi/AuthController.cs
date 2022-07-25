using Sentry.data.Core;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Web.Http;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_AUTH)]
    //Users need aleast UseApp permission to access any endpoint on this controller
    [WebApiAuthorizeUseApp]
    public class AuthController : BaseWebApiController
    {
        private readonly IDatasetContext _dsContext;
        private readonly UserService _userService;
        private readonly ISecurityService _securityService;

        public AuthController(IDatasetContext dsContext, UserService userService, ISecurityService securityService, IMessagePublisher messagePublisher)
        {
            _dsContext = dsContext;
            _userService = userService;
            _securityService = securityService;
        }

        /// <summary>
        /// Creates default security for dataset IDs provided
        /// </summary>
        [HttpPost]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [WebApiAuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
        [Route("CreateDefaultSecurity")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        public IHttpActionResult CreateDefaultSecurity(int[] idList)
        {
            Sentry.Common.Logging.Logger.Info($"{_userService.GetCurrentRealUser().AssociateId} called CreateDefaultSecurity on Dataset Id(s) {idList}");
            //validate
            foreach (int datasetId in idList)
            {
                var ds = _dsContext.GetById<Dataset>(datasetId);
                if (ds == null)
                {
                    return BadRequest($"Dataset with ID \"{datasetId}\" could not be found.");
                }
            }
            //queue jobs
            _securityService.EnqueueCreateDefaultSecurityForDatasetList(idList);
            return Ok();
        }

    }
}

using Sentry.data.Core;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Linq;
using System.Web.Http;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_DATASOURCE)]
    //Users need aleast UseApp permission to access any endpoint on this controller
    [WebApiAuthorizeUseApp]
    public class DataSourceController : BaseWebApiController
    {
        private readonly IDatasetContext _dsContext;
        private readonly UserService _userService;


        public DataSourceController(IDatasetContext dsContext, UserService userService)
        {
            _dsContext = dsContext;
            _userService = userService;
        }

        /// <summary>
        /// Creates default security for dataset IDs provided
        /// </summary>
        [HttpPost]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [Route("{dataSourceId}/{authToken}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK)]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        public IHttpActionResult AddAccessTokenFromAuthToken(int dataSourceId, string authToken)
        {
            //validate
            if (authToken == null)
            {
                return BadRequest($"No Auth Token provided.");
            }
            var dataSource = _dsContext.DataSources.FirstOrDefault(ds => ds.Id == dataSourceId);
            if (dataSource == null)
            {
                return BadRequest($"Datasource with ID \"{dataSourceId}\" could not be found.");
            }

            //make call to exchange token

            //add token to datasource

            //drop companies file

            //kick off backfill
            return Ok();
        }
    }
}

using Sentry.data.Core;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Threading.Tasks;
using System.Web.Http;
using Sentry.data.Core.Exceptions;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_METADATA)]
    //Users need aleast UseApp permission to access any endpoint on this controller
    [WebApiAuthorizeUseApp]
    public class DataInventoryController : BaseWebApiController
    {

        private readonly IDaleService _daleService;


        public DataInventoryController(IDaleService daleService)
        {
            _daleService = daleService;
        }

        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("DoesItContainSensitive")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(bool))]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> DoesItemContainSensitive(string target, string search)
        {
            DaleContainSensitiveResultModel resultModel ;

            try
            {
                DaleSearchModel searchModel = new DaleSearchModel();
                searchModel.Criteria = search;
                searchModel.Destiny = target.ToDaleDestiny();

                resultModel = _daleService.DoesItemContainSensitive(searchModel.ToDto()).ToWeb();

                return Ok(resultModel.DoesContainSensitiveResults);

            }
            catch (DaleUnauthorizedAccessException)
            {
                return Content(System.Net.HttpStatusCode.Forbidden, "Not Authorized");
            }
            catch (DaleQueryException)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, "Invalid target or search parameters");
            }

        }
    }
}

using Sentry.data.Core;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Threading.Tasks;
using System.Web.Http;
using Sentry.data.Core.Exceptions;
using System.Collections.Generic;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_METADATA)]
    //Users need aleast UseApp permission to access any endpoint on this controller
    [WebApiAuthorizeUseApp]
    public class DataInventoryController : BaseWebApiController
    {

        private readonly IDataInventoryService _dataInventoryService;


        public DataInventoryController(IDataInventoryService dataInventoryService)
        {
            _dataInventoryService = dataInventoryService;
        }

        [HttpGet]
        [ApiVersionBegin(WebAPI.Version.v2)]
        [Route("DoesItemContainSensitive")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(bool))]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> DoesItemContainSensitive(string target, string search)
        {
            try
            {
                DataInventorySensitiveSearchDto dto = new DataInventorySensitiveSearchDto()
                {
                    SearchText = search,
                    SearchTarget = target
                };

                bool containsSensitive = _dataInventoryService.DoesItemContainSensitive(dto);
                return Ok(containsSensitive);
            }
            catch (DataInventoryUnauthorizedAccessException)
            {
                return Content(System.Net.HttpStatusCode.Forbidden, "Not Authorized.");
            }
            catch (DataInventoryQueryException)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, "DB Query Failure.");
            }
            catch (DataInventoryInvalidSearchException)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, "Invalid target or search parameters");
            }

        }

        [HttpGet]
        [ApiVersionBegin(WebAPI.Version.v2)]
        [Route("GetCategoriesByAsset")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(List<DaleCategoryModel>))]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> GetCategoriesByAsset(string search)
        {
            try
            {
                List<DaleCategoryModel> result = _dataInventoryService.GetCategoriesByAsset(search).ToWeb();
                return Ok(result);
            }
            catch (DataInventoryUnauthorizedAccessException)
            {
                return Content(System.Net.HttpStatusCode.Forbidden, "Not Authorized.");
            }
            catch (DataInventoryQueryException)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, "DB Query Failure.");
            }
            catch (DataInventoryInvalidSearchException)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, "Invalid target or search parameters");
            }

        }
    }
}

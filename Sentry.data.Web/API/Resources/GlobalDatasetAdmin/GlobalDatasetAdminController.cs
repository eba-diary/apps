using Sentry.data.Core;
using Sentry.data.Web.WebApi;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.API
{
    [RoutePrefix(WebConstants.Routes.VERSION_GLOBALDATASETS_ADMIN)]
    [WebApiAuthorizeUseApp]
    public class GlobalDatasetAdminController : BaseApiController<GlobalDatasetAdminController>
    {
        private readonly IGlobalDatasetAdminService _globalDatasetAdminService;

        public GlobalDatasetAdminController(IGlobalDatasetAdminService globalDatasetAdminService, ApiCommonDependency<GlobalDatasetAdminController> apiDependency) : base(apiDependency)
        {
            _globalDatasetAdminService = globalDatasetAdminService;
        }

        /// <summary>
        /// Index global datasets by IDs
        /// </summary>
        [HttpPost]
        [Route("index")]
        [ApiVersionBegin(WebAPI.Version.v20230315)]
        [SwaggerResponse(HttpStatusCode.Created, null, typeof(IndexGlobalDatasetsResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.ServiceUnavailable)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> IndexGlobalDatasets(IndexGlobalDatasetsRequestModel model)
        {
            return await ProcessRequestAsync<IndexGlobalDatasetsRequestModel, IndexGlobalDatasetsDto, IndexGlobalDatasetsResultDto, IndexGlobalDatasetsResponseModel>(model, _globalDatasetAdminService.IndexGlobalDatasetsAsync);
        }
    }
}

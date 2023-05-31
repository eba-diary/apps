using AutoMapper;
using Sentry.data.Core;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.API
{
    [RoutePrefix(WebConstants.Routes.VERSION_GLOBALDATASETS)]
    public class GlobalDatasetController : BaseApiController
    {
        private readonly IGlobalDatasetService _globalDatasetService;

        public GlobalDatasetController(IGlobalDatasetService globalDatasetService, IApiDependency apiDependency) : base(apiDependency)
        {
            _globalDatasetService = globalDatasetService;
        }

        /// <summary>
        /// Search global datasets
        /// </summary>
        [HttpPost]
        [Route("search")]
        [ApiVersionBegin(WebAPI.Version.v20230315)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(SearchGlobalDatasetsResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.ServiceUnavailable)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> SearchGlobalDatasets(SearchGlobalDatasetsRequestModel model)
        {
            return await ProcessRequestAsync<SearchGlobalDatasetsRequestModel, SearchGlobalDatasetsDto, SearchGlobalDatasetsResultsDto, SearchGlobalDatasetsResponseModel>(model, _globalDatasetService.SearchGlobalDatasetsAsync);
        }

        /// <summary>
        /// Aggregate search filters for global dataset search
        /// </summary>
        [HttpPost]
        [Route("filters")]
        [ApiVersionBegin(WebAPI.Version.v20230315)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(GetGlobalDatasetFiltersResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.ServiceUnavailable)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetGlobalDatasetFilters(GetGlobalDatasetFiltersRequestModel model)
        {
            return await ProcessRequestAsync<GetGlobalDatasetFiltersRequestModel, GetGlobalDatasetFiltersDto, GetGlobalDatasetFiltersResultDto, GetGlobalDatasetFiltersResponseModel>(model, _globalDatasetService.GetGlobalDatasetFiltersAsync);
        }
    }
}

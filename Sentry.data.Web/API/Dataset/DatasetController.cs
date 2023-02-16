using AutoMapper;
using Sentry.data.Core;
using Sentry.data.Web.WebApi;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.API
{
    [RoutePrefix(WebConstants.Routes.VERSION_DATASETS)]
    [WebApiAuthorizeUseApp]
    public class DatasetController : BaseApiController
    {
        private readonly IDatasetService _datasetService;

        public DatasetController(IDatasetService datasetService, IMapper mapper, IValidationRegistry validationRegistry) : base(mapper, validationRegistry)
        {
            _datasetService = datasetService;
        }

        /// <summary>
        /// Create dataset
        /// </summary>
        [HttpPost]
        [Route("")]
        [ApiVersionBegin(WebAPI.Version.v20230223)]
        [SwaggerResponse(HttpStatusCode.Created, null, typeof(AddDatasetResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> CreateDataset(AddDatasetRequestModel model)
        {
            return await ProcessRequestAsync<AddDatasetRequestModel, DatasetDto, DatasetResultDto, AddDatasetResponseModel>(model, _datasetService.AddDatasetAsync);
        }
    }
}
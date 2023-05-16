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

        public DatasetController(IDatasetService datasetService, IMapper mapper, IValidationRegistry validationRegistry, IDataFeatures dataFeatures) : base(mapper, validationRegistry, dataFeatures)
        {
            _datasetService = datasetService;
        }

        /// <summary>
        /// Add new dataset
        /// </summary>
        [HttpPost]
        [Route("")]
        [ApiVersionBegin(WebAPI.Version.v20230315)]
        [SwaggerResponse(HttpStatusCode.Created, null, typeof(AddDatasetResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> AddDataset(AddDatasetRequestModel model)
        {
            return await ProcessRequestAsync<AddDatasetRequestModel, DatasetDto, DatasetResultDto, AddDatasetResponseModel>(model, _datasetService.AddDatasetAsync);
        }

        /// <summary>
        /// Update existing dataset
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        [ApiVersionBegin(WebAPI.Version.v20230315)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(UpdateDatasetResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> UpdateDataset(int id, UpdateDatasetRequestModel model)
        {
            return await ProcessRequestAsync<UpdateDatasetRequestModel, DatasetDto, DatasetResultDto, UpdateDatasetResponseModel>(id, model, _datasetService.UpdateDatasetAsync);
        }
    }
}
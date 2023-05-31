using Sentry.data.Core;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.API
{
    [RoutePrefix(WebConstants.Routes.VERSION_ASSISTANCE)]
    public class AssistanceController : BaseApiController
    {
        private readonly IAssistanceService _assistanceService;

        public AssistanceController(IAssistanceService assistanceService, IApiDependency apiDependency) : base(apiDependency)
        {
            _assistanceService = assistanceService;
        }

        /// <summary>
        /// Search global datasets
        /// </summary>
        [HttpPost]
        [Route("")]
        [ApiVersionBegin(WebAPI.Version.v20230315)]
        [SwaggerResponse(HttpStatusCode.Created, null, typeof(AddAssistanceResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.ServiceUnavailable)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> AddAssistance(AddAssistanceRequestModel model)
        {
            return await ProcessRequestAsync<AddAssistanceRequestModel, AddAssistanceDto, AddAssistanceResultDto, AddAssistanceResponseModel>(model, _assistanceService.AddAssistanceAsync);
        }
    }
}

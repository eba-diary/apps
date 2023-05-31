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
    [RoutePrefix(WebConstants.Routes.VERSION_SCHEMAS)]
    [WebApiAuthorizeUseApp]
    public class SchemaController : BaseApiController
    {
        private readonly ISchemaFlowService _schemaFlowService;

        public SchemaController(ISchemaFlowService schemaFlowService, IApiDependency apiDependency) : base(apiDependency)
        {
            _schemaFlowService = schemaFlowService;
        }

        /// <summary>
        /// Add new schema
        /// </summary>
        [HttpPost]
        [Route("")]
        [ApiVersionBegin(WebAPI.Version.v20230315)]
        [SwaggerResponse(HttpStatusCode.Created, null, typeof(AddSchemaResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> AddSchema(AddSchemaRequestModel model)
        {
            return await ProcessRequestAsync<AddSchemaRequestModel, SchemaFlowDto, SchemaResultDto, AddSchemaResponseModel>(model, _schemaFlowService.AddSchemaAsync);
        }

        /// <summary>
        /// Update existing schema
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        [ApiVersionBegin(WebAPI.Version.v20230315)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(UpdateSchemaResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> UpdateSchema(int id, UpdateSchemaRequestModel model)
        {
            return await ProcessRequestAsync<UpdateSchemaRequestModel, SchemaFlowDto, SchemaResultDto, UpdateSchemaResponseModel>(id, model, _schemaFlowService.UpdateSchemaAsync);
        }
    }
}
using AutoMapper;
using Sentry.data.Core;
using Sentry.data.Web.WebApi;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Sentry.data.Web.API
{
    [RoutePrefix(WebConstants.Routes.VERSION_SCHEMAS)]
    [WebApiAuthorizeUseApp]
    public class SchemaController : BaseApiController
    {
        private readonly ISchemaFlowService _schemaFlowService;

        public SchemaController(ISchemaFlowService schemaFlowService, IMapper mapper, IValidationRegistry validationRegistry) : base(mapper, validationRegistry)
        {
            _schemaFlowService = schemaFlowService;
        }

        /// <summary>
        /// Add new schema
        /// </summary>
        [HttpPost]
        [Route("")]
        [ApiVersionBegin(WebAPI.Version.v20230223)]
        [SwaggerResponse(HttpStatusCode.Created, null, typeof(AddSchemaResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> AddSchema(AddSchemaRequestModel model)
        {
            return await ProcessRequestAsync<AddSchemaRequestModel, AddSchemaDto, SchemaResultDto, AddSchemaResponseModel>(model, _schemaFlowService.AddSchemaAsync);
        }

        /// <summary>
        /// Update existing schema
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        [ApiVersionBegin(WebAPI.Version.v20230223)]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(UpdateSchemaResponseModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, null, typeof(ValidationResponseModel))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> UpdateSchema(int id, UpdateSchemaRequestModel model)
        {
            return await ProcessRequestAsync<UpdateSchemaRequestModel, UpdateSchemaDto, SchemaResultDto, UpdateSchemaResponseModel>(id, model, _schemaFlowService.UpdateSchemaAsync);
        }
    }
}
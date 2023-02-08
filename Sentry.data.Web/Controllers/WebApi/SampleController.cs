using AutoMapper;
using Sentry.data.Core;
using Sentry.data.Web.WebApi;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.Controllers.WebApi
{
    [RoutePrefix(WebConstants.Routes.VERSION_SAMPLES)]
    [WebApiAuthorizeUseApp]
    public class SampleController : BaseApiController
    {
        private readonly ISampleService _sampleService;

        public SampleController(ISampleService sampleService, IMapper mapper, IValidationRegistry validationRegistry) : base(mapper, validationRegistry) 
        {
            _sampleService = sampleService;
        }

        /// <summary>
        /// Get sample by Id
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(GetSampleResponseViewModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.NotFound)]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetSample(int id)
        {
            return await ProcessRequestAsync<SampleDto, GetSampleResponseViewModel>(id, _sampleService.GetSample);
        }

        /// <summary>
        /// Add sample
        /// </summary>
        [HttpPost]
        [Route("")]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [SwaggerResponse(System.Net.HttpStatusCode.Created, null, typeof(AddSampleResponseViewModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, null, typeof(List<ValidationResultViewModel>))]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> AddSample(AddSampleViewModel model)
        {
            return await ProcessRequestAsync<AddSampleViewModel, SampleDto, AddSampleResponseViewModel>(model, _sampleService.AddSample);
        }

        /// <summary>
        /// Update sample by Id
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        [ApiVersionBegin(WebAPI.Version.v20220609)]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(UpdateSampleResponseViewModel))]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, null, typeof(List<ValidationResultViewModel>))]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> UpdateSample(int id, UpdateSampleViewModel model)
        {
            return await ProcessRequestAsync<UpdateSampleViewModel, SampleDto, UpdateSampleResponseViewModel>(id, model, _sampleService.UpdateSample);
        }
    }
}
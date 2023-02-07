using AutoMapper;
using Sentry.data.Core;
using Sentry.data.Web.WebApi;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.Controllers.WebApi
{
    [RoutePrefix(WebConstants.Routes.VERSION_SAMPLE)]
    [ApiVersionBegin(WebAPI.Version.v20220609)]
    [WebApiAuthorizeUseApp]
    public class SampleController : BaseApiController
    {
        private readonly ISampleService _sampleService;

        public SampleController(ISampleService sampleService, IMapper mapper, IDictionary<Type, Lazy<IDtoValidator<IValidatableDto>>> validators) : base(mapper, validators) 
        {
            _sampleService = sampleService;
        }

        [HttpPost]
        [Route("samples")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SampleResultViewModel))]
        public async Task<IHttpActionResult> AddSample(AddSampleViewModel model)
        {
            return await ProcessRequestAsync<AddSampleViewModel, SampleDto, SampleResultViewModel>(model, _sampleService.AddSample);
        }

        [HttpPut]
        [Route("samples/{id}")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(SampleResultViewModel))]
        public async Task<IHttpActionResult> UpdateSample(int id, UpdateSampleViewModel model)
        {
            return await ProcessRequestAsync<SampleViewModel, SampleDto, SampleDto, SampleViewModel>(id, model, _sampleService.UpdateSample);
        }
    }
}
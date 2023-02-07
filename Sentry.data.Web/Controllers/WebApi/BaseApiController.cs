using AutoMapper;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Sentry.data.Web.Controllers.WebApi
{
    public class BaseApiController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly IDictionary<Type, Lazy<IDtoValidator<IValidatableDto>>> _validators;

        protected BaseApiController(IMapper mapper, IDictionary<Type, Lazy<IDtoValidator<IValidatableDto>>> validators)
        {
            _mapper = mapper;
            _validators = validators;
        }

        protected async Task<IHttpActionResult> ProcessRequestAsync<reqT, dtoT, respT>(int id, reqT requestModel, Func<dtoT, Task<dtoT>> service) where dtoT : IIdentifiableDto
        {
            return await ProcessRequestAsync<reqT, dtoT, dtoT, respT>(id, requestModel, service);
        }

        protected async Task<IHttpActionResult> ProcessRequestAsync<reqT, inT, outT, respT>(int id, reqT requestModel, Func<inT, Task<outT>> service) where inT : IIdentifiableDto
        {
            try
            {
                inT dto = _mapper.Map<inT>(requestModel);
                dto.SetId(id);

                return await ProcessRequestAsync<inT, outT, respT>(dto, service);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        protected async Task<IHttpActionResult> ProcessRequestAsync<reqT, dtoT, respT>(reqT requestModel, Func<dtoT, Task<dtoT>> service) where dtoT : IValidatableDto
        {
            return await ProcessRequestAsync<reqT, dtoT, dtoT, respT>(requestModel, service);
        }

        protected async Task<IHttpActionResult> ProcessRequestAsync<reqT, inT, outT, respT>(reqT requestModel, Func<inT, Task<outT>> service) where inT : IValidatableDto
        {
            try
            {
                inT dto = _mapper.Map<inT>(requestModel);

                return await ProcessRequestAsync<inT, outT, respT>(dto, service);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        private async Task<IHttpActionResult> ProcessRequestAsync<inT, outT, respT>(inT dto, Func<inT, Task<outT>> service) where inT : IValidatableDto
        {
            if (_validators.ContainsKey(typeof(inT)))
            {
                List<ValidationResultDto> validationResults = _validators[typeof(inT)].Value.Validate(dto);
                if (validationResults?.Any() == true)
                {
                    List<ValidationResultViewModel> validationResultModels = _mapper.Map<List<ValidationResultViewModel>>(validationResults);
                    return Content(HttpStatusCode.BadRequest, validationResultModels);
                }
            }

            outT result = await service(dto);

            respT response = _mapper.Map<respT>(result);

            return Ok(response);
        }
    }
}

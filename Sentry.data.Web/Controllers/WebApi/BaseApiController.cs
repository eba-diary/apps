using AutoMapper;
using Sentry.data.Core;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Sentry.data.Web.Controllers.WebApi
{
    public abstract class BaseApiController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly IValidationRegistry _validationRegistry;

        protected BaseApiController(IMapper mapper, IValidationRegistry validationRegistry)
        {
            _mapper = mapper;
            _validationRegistry = validationRegistry;
        }

        /// <summary>
        /// <list type="number">
        /// <item><description>Executes service method that accepts a resource's ID</description></item>
        /// <item><description>Maps service method's resulting DTO to response view model</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="dtoOut">Type of service method's OUT DTO</typeparam>
        /// <typeparam name="modelOut">Type of response view model</typeparam>
        /// <param name="id">ID of the resource to be used by service method</param>
        /// <param name="service">Service method that takes single integer parameter to execute</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<dtoOut, modelOut>(int id, Func<int, Task<dtoOut>> service) where modelOut : IResponseModel
        {
            return await Catch(async () => await GetResponse<int, dtoOut, modelOut>(id, service));
        }

        /// <summary>
        /// <list type="number">
        /// <item><description>Validates the request</description></item>
        /// <item><description>Maps request view model to DTO accepted by service method</description></item>
        /// <item><description>Sets the ID of the DTO using the id parameter</description></item>
        /// <item><description>Executes service method</description></item>
        /// <item><description>Maps service method's resulting DTO to response view model</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="modelIn">Type of request view model</typeparam>
        /// <typeparam name="dtoInOut">Type of service method's IN and OUT DTO</typeparam>
        /// <typeparam name="modelOut">Type of response view model</typeparam>
        /// <param name="id">ID of the resource to be used by service method</param>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with same IN and OUT DTO type</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<modelIn, dtoInOut, modelOut>(int id, modelIn requestModel, Func<dtoInOut, Task<dtoInOut>> service) where modelIn: IRequestModel where dtoInOut : IIdentifiableDto where modelOut : IResponseModel
        {
            return await ProcessRequestAsync<modelIn, dtoInOut, dtoInOut, modelOut>(id, requestModel, service);
        }

        /// <summary>
        /// <list type="number">
        /// <item><description>Validates the request</description></item>
        /// <item><description>Maps request view model to DTO accepted by service method</description></item>
        /// <item><description>Sets the ID of the DTO using the id parameter</description></item>
        /// <item><description>Executes service method</description></item>
        /// <item><description>Maps service method's resulting DTO to response view model</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="modelIn">Type of request view model</typeparam>
        /// <typeparam name="dtoIn">Type of service method's IN DTO</typeparam>
        /// <typeparam name="dtoOut">Type of service method's OUT DTO</typeparam>
        /// <typeparam name="modelOut">Type of response view model</typeparam>
        /// <param name="id">ID of the resource to be used by service method</param>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with different IN and OUT DTO types</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<modelIn, dtoIn, dtoOut, modelOut>(int id, modelIn requestModel, Func<dtoIn, Task<dtoOut>> service) where modelIn : IRequestModel where dtoIn : IIdentifiableDto where modelOut : IResponseModel
        {
            return await Catch(async () =>
            {
                dtoIn dtoInput = MapValidatedViewModel<modelIn, dtoIn>(requestModel);
                dtoInput.SetId(id);
                return await GetResponse<dtoIn, dtoOut, modelOut>(dtoInput, service);
            });
        }

        /// <summary>
        /// <list type="number">
        /// <item><description>Validates the request</description></item>
        /// <item><description>Maps request view model to DTO accepted by service method</description></item>
        /// <item><description>Executes service method</description></item>
        /// <item><description>Maps service method's resulting DTO to response view model</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="modelIn">Type of request view model</typeparam>
        /// <typeparam name="dtoInOut">Type of service method's IN and OUT DTO</typeparam>
        /// <typeparam name="modelOut">Type of response view model</typeparam>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with same IN and OUT DTO type</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<modelIn, dtoInOut, modelOut>(modelIn requestModel, Func<dtoInOut, Task<dtoInOut>> service) where modelIn : IRequestModel where modelOut : IResponseModel
        {
            return await ProcessRequestAsync<modelIn, dtoInOut, dtoInOut, modelOut>(requestModel, service);
        }

        /// <summary>
        /// <list type="number">
        /// <item><description>Validates the request</description></item>
        /// <item><description>Maps request view model to DTO accepted by service method</description></item>
        /// <item><description>Executes service method</description></item>
        /// <item><description>Maps service method's resulting DTO to response view model</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="modelIn">Type of request view model</typeparam>
        /// <typeparam name="dtoIn">Type of service method's IN DTO</typeparam>
        /// <typeparam name="dtoOut">Type of service method's OUT DTO</typeparam>
        /// <typeparam name="modelOut">Type of response view model</typeparam>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with different IN and OUT DTO types</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<modelIn, dtoIn, dtoOut, modelOut>(modelIn requestModel, Func<dtoIn, Task<dtoOut>> service) where modelIn : IRequestModel where modelOut : IResponseModel
        {
            return await Catch(async () =>
            {
                dtoIn dtoInput = MapValidatedViewModel<modelIn, dtoIn>(requestModel);
                return await GetResponse<dtoIn, dtoOut, modelOut>(dtoInput, service);
            });
        }

        #region Private
        private dtoIn MapValidatedViewModel<modelIn, dtoIn>(modelIn requestModel) where modelIn : IRequestModel
        {
            if (requestModel == null)
            {
                throw new RequestModelValidationException();
            }

            if (_validationRegistry.TryGetValidatorFor<modelIn>(out IRequestModelValidator validator))
            {
                ValidationResponseModel validationResponse = validator.Validate(requestModel);

                if (!validationResponse.IsValid())
                {
                    throw new RequestModelValidationException(validationResponse);
                }
            }

            dtoIn dto = _mapper.Map<dtoIn>(requestModel);
            return dto;
        }

        private async Task<IHttpActionResult> GetResponse<dtoIn, dtoOut, modelOut>(dtoIn dtoInput, Func<dtoIn, Task<dtoOut>> service) where modelOut : IResponseModel
        {
            dtoOut dtoOutput = await service(dtoInput);
            return MapToResult<dtoOut, modelOut>(dtoOutput);
        }

        private IHttpActionResult MapToResult<dtoOut, modelOut>(dtoOut result) where modelOut : IResponseModel
        {
            modelOut response = _mapper.Map<modelOut>(result);

            if (HttpContext.Current.Request.HttpMethod == "POST")
            {
                return Content(HttpStatusCode.Created, response);
            }
            else
            {
                return Ok(response);
            }
        }

        private async Task<IHttpActionResult> Catch(Func<Task<IHttpActionResult>> getResult)
        {
            try
            {
                return await getResult();
            }
            catch (RequestModelValidationException vmve)
            {
                if (vmve.ValidationResponse != null)
                {
                    return Content(HttpStatusCode.BadRequest, vmve.ValidationResponse);
                }

                return StatusCode(HttpStatusCode.BadRequest);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
            catch (ResourceForbiddenException)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
        #endregion
    }
}

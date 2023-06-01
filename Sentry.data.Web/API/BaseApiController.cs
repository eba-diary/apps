using AutoMapper;
using Microsoft.Extensions.Logging;
using Sentry.data.Core;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Sentry.data.Web.API
{
    public abstract class BaseApiController<T> : ApiController
    {
        private readonly IMapper _mapper;
        private readonly IValidationRegistry _validationRegistry;
        private readonly IDataFeatures _dataFeatures;
        private readonly ILogger<T> _logger;

        protected BaseApiController(ApiCommonDependency<T> apiCommonDependency)
        {
            _mapper = apiCommonDependency.Mapper;
            _validationRegistry = apiCommonDependency.ValidationRegistry;
            _logger = apiCommonDependency.Logger;
            _dataFeatures = apiCommonDependency.DataFeatures;
        }

        /// <summary>
        /// <list type="number">
        /// <item><description>Executes service method that accepts a resource's ID</description></item>
        /// <item><description>Maps service method's resulting DTO to response view model</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="dtoOut">Output type of service method</typeparam>
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
        /// <typeparam name="dtoIn">Input type for the service method</typeparam>
        /// <typeparam name="dtoOut">Output type of service method</typeparam>
        /// <typeparam name="modelOut">Type of response view model</typeparam>
        /// <param name="id">ID of the resource to be used by service method</param>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with different IN and OUT DTO types</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<modelIn, dtoIn, dtoOut, modelOut>(int id, modelIn requestModel, Func<dtoIn, Task<dtoOut>> service) where modelIn : IRequestModel where dtoIn : IIdentifiableDto where modelOut : IResponseModel
        {
            return await Catch(async () =>
            {
                dtoIn dtoInput = await MapValidatedViewModel<modelIn, dtoIn>(requestModel);
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
        /// <typeparam name="dtoInOut">Input and output type for the service method</typeparam>
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
        /// <typeparam name="dtoIn">Input type for the service method</typeparam>
        /// <typeparam name="dtoOut">Output type of service method</typeparam>
        /// <typeparam name="modelOut">Type of response view model</typeparam>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with different IN and OUT DTO types</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<modelIn, dtoIn, dtoOut, modelOut>(modelIn requestModel, Func<dtoIn, Task<dtoOut>> service) where modelIn : IRequestModel where modelOut : IResponseModel
        {
            return await Catch(async () =>
            {
                dtoIn dtoInput = await MapValidatedViewModel<modelIn, dtoIn>(requestModel);
                return await GetResponse<dtoIn, dtoOut, modelOut>(dtoInput, service);
            });
        }

        #region Private
        private async Task<dtoIn> MapValidatedViewModel<modelIn, dtoIn>(modelIn requestModel) where modelIn : IRequestModel
        {
            if (requestModel == null)
            {
                throw new RequestModelValidationException();
            }

            if (_validationRegistry.TryGetValidatorFor<modelIn>(out IRequestModelValidator validator))
            {
                ConcurrentValidationResponse validationResponse = await validator.ValidateAsync(requestModel);

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
                if (_dataFeatures.CLA4912_API.GetValue())
                {
                    return await getResult();
                }
                else
                {
                    return StatusCode(HttpStatusCode.ServiceUnavailable);
                }
            }
            catch (RequestModelValidationException vmve)
            {
                if (vmve.ValidationResponse != null)
                {
                    ValidationResponseModel validationResponseModel = _mapper.Map<ValidationResponseModel>(vmve.ValidationResponse);
                    return Content(HttpStatusCode.BadRequest, validationResponseModel);
                }

                return StatusCode(HttpStatusCode.BadRequest);
            }
            catch (ResourceNotFoundException notFound)
            {
                _logger.LogInformation($"API - Resource Id {notFound.ResourceId} not found for {notFound.ResourceAction}");
                return NotFound();
            }
            catch (ResourceForbiddenException forbidden)
            {
                _logger.LogInformation($"API - {forbidden.UserId} does not have {forbidden.Permission} permission to {forbidden.ResourceAction} | Resource Id: {forbidden.ResourceId}");
                return StatusCode(HttpStatusCode.Forbidden);
            }
            catch (ResourceFeatureDisabledException featureNotEnabled)
            {
                _logger.LogInformation($"API - {featureNotEnabled.FeatureFlagName} not enabled to perform {featureNotEnabled.ResourceAction}");
                return StatusCode(HttpStatusCode.ServiceUnavailable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"API - Unexpected error occurred during API request");
                return InternalServerError(e);
            }
        }
        #endregion
    }
}

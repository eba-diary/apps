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
        /// <typeparam name="viewModelOut">Type of response view model</typeparam>
        /// <param name="id">ID of the resource to be used by service method</param>
        /// <param name="service">Service method that takes single integer parameter to execute</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<dtoOut, viewModelOut>(int id, Func<int, Task<dtoOut>> service) where viewModelOut : IResponseViewModel
        {
            return await Catch(async () => await GetResponse<int, dtoOut, viewModelOut>(id, service));
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
        /// <typeparam name="viewModelIn">Type of request view model</typeparam>
        /// <typeparam name="dtoInOut">Type of service method's IN and OUT DTO</typeparam>
        /// <typeparam name="viewModelOut">Type of response view model</typeparam>
        /// <param name="id">ID of the resource to be used by service method</param>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with same IN and OUT DTO type</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<viewModelIn, dtoInOut, viewModelOut>(int id, viewModelIn requestModel, Func<dtoInOut, Task<dtoInOut>> service) where viewModelIn: IRequestViewModel where dtoInOut : IIdentifiableDto where viewModelOut : IResponseViewModel
        {
            return await ProcessRequestAsync<viewModelIn, dtoInOut, dtoInOut, viewModelOut>(id, requestModel, service);
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
        /// <typeparam name="viewModelIn">Type of request view model</typeparam>
        /// <typeparam name="dtoIn">Type of service method's IN DTO</typeparam>
        /// <typeparam name="dtoOut">Type of service method's OUT DTO</typeparam>
        /// <typeparam name="viewModelOut">Type of response view model</typeparam>
        /// <param name="id">ID of the resource to be used by service method</param>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with different IN and OUT DTO types</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<viewModelIn, dtoIn, dtoOut, viewModelOut>(int id, viewModelIn requestModel, Func<dtoIn, Task<dtoOut>> service) where viewModelIn : IRequestViewModel where dtoIn : IIdentifiableDto where viewModelOut : IResponseViewModel
        {
            return await Catch(async () =>
            {
                dtoIn dtoInput = MapValidatedViewModel<viewModelIn, dtoIn>(requestModel);
                dtoInput.SetId(id);
                return await GetResponse<dtoIn, dtoOut, viewModelOut>(dtoInput, service);
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
        /// <typeparam name="viewModelIn">Type of request view model</typeparam>
        /// <typeparam name="dtoInOut">Type of service method's IN and OUT DTO</typeparam>
        /// <typeparam name="viewModelOut">Type of response view model</typeparam>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with same IN and OUT DTO type</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<viewModelIn, dtoInOut, viewModelOut>(viewModelIn requestModel, Func<dtoInOut, Task<dtoInOut>> service) where viewModelIn : IRequestViewModel where viewModelOut : IResponseViewModel
        {
            return await ProcessRequestAsync<viewModelIn, dtoInOut, dtoInOut, viewModelOut>(requestModel, service);
        }

        /// <summary>
        /// <list type="number">
        /// <item><description>Validates the request</description></item>
        /// <item><description>Maps request view model to DTO accepted by service method</description></item>
        /// <item><description>Executes service method</description></item>
        /// <item><description>Maps service method's resulting DTO to response view model</description></item>
        /// </list>
        /// </summary>
        /// <typeparam name="viewModelIn">Type of request view model</typeparam>
        /// <typeparam name="dtoIn">Type of service method's IN DTO</typeparam>
        /// <typeparam name="dtoOut">Type of service method's OUT DTO</typeparam>
        /// <typeparam name="viewModelOut">Type of response view model</typeparam>
        /// <param name="requestModel">Request view model</param>
        /// <param name="service">Service method with different IN and OUT DTO types</param>
        /// <returns>IHttpActionResult instance with proper HTTP status code according to HTTP specifications</returns>
        protected async Task<IHttpActionResult> ProcessRequestAsync<viewModelIn, dtoIn, dtoOut, viewModelOut>(viewModelIn requestModel, Func<dtoIn, Task<dtoOut>> service) where viewModelIn : IRequestViewModel where viewModelOut : IResponseViewModel
        {
            return await Catch(async () =>
            {
                dtoIn dtoInput = MapValidatedViewModel<viewModelIn, dtoIn>(requestModel);
                return await GetResponse<dtoIn, dtoOut, viewModelOut>(dtoInput, service);
            });
        }

        #region Private
        private dtoIn MapValidatedViewModel<viewModelIn, dtoIn>(viewModelIn requestModel) where viewModelIn : IRequestViewModel
        {
            if (requestModel == null)
            {
                throw new ViewModelValidationException();
            }

            if (_validationRegistry.TryGetValidatorFor<viewModelIn>(out IViewModelValidator validator))
            {
                validator.Validate(requestModel);
            }

            dtoIn dto = _mapper.Map<dtoIn>(requestModel);
            return dto;
        }

        private async Task<IHttpActionResult> GetResponse<dtoIn, dtoOut, viewModelOut>(dtoIn dtoInput, Func<dtoIn, Task<dtoOut>> service) where viewModelOut : IResponseViewModel
        {
            dtoOut dtoOutput = await service(dtoInput);
            return MapToResult<dtoOut, viewModelOut>(dtoOutput);
        }

        private IHttpActionResult MapToResult<dtoOut, viewModelOut>(dtoOut result) where viewModelOut : IResponseViewModel
        {
            viewModelOut response = _mapper.Map<viewModelOut>(result);
            response.SetLinks();

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
            catch (ViewModelValidationException vmve)
            {
                if (vmve.ValidationResults?.Any() == true)
                {
                    return Content(HttpStatusCode.BadRequest, vmve.ValidationResults);
                }

                return StatusCode(HttpStatusCode.BadRequest);
            }
            catch (ResourceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
        #endregion
    }
}

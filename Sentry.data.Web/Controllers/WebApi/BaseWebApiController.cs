using Sentry.data.Core.Exceptions;
using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Sentry.Common.Logging;

namespace Sentry.data.Web.WebApi.Controllers
{
    public class BaseWebApiController : ApiController
    {


        protected StatusCodeResult NoContent()
        {
            return StatusCode(HttpStatusCode.NoContent);
        }


        /// <summary>
        /// Handles catching the following Core exceptions:
        ///   DatasetNotFoundException
        ///   SchemaNotFoundException
        ///   DatasetUnauthorizedAccessException
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="methodName"></param>
        /// <param name="errorMetadata"></param>
        /// <param name="myMethod1"></param>
        /// <returns></returns>
        /// <exception cref="System.Net.HttpStatusCode.NotFound">Thrown when dataset or schema not found</exception>
        /// <exception cref="System.Net.HttpStatusCode.Forbidden">Thrown when user does not have access to dataset or schema</exception>
        /// <exception cref="System.Net.HttpStatusCode.InternalServerError">Thrown when an unhandled exception occurs</exception>
        /// <exception cref="System.Net.HttpStatusCode.BadRequest">Thrown when an unhandled exception occurs</exception>
        protected IHttpActionResult ApiTryCatch(string controllerName, string methodName, string errorMetadata, Func<IHttpActionResult> myMethod1)
        {
            //return ApiTryCatch(controllerName, methodName, errorMetadata, () => Task.Run(myMethod1)).GetAwaiter().GetResult();

            try
            {
                return myMethod1();
            }
            catch (DatasetNotFoundException)
            {
                Logger.Debug($"{controllerName.ToLower()}_{methodName.ToLower()}_notfound dataset - {errorMetadata}");
                return Content(System.Net.HttpStatusCode.NotFound, "Dataset not found");
            }
            catch (SchemaNotFoundException)
            {
                Logger.Debug($"{controllerName.ToLower()}_{methodName.ToLower()}_notfound schema - {errorMetadata}");
                return Content(System.Net.HttpStatusCode.NotFound, "Schema not found");
            }
            catch (SchemaRevisionNotFoundException)
            {
                Logger.Debug($"{controllerName.ToLower()}_{methodName.ToLower()}_notfound schemaRevision - {errorMetadata}");
                return Content(System.Net.HttpStatusCode.Forbidden, "Schema Revision not found");
            }
            catch (DataFileNotFoundException)
            {
                Logger.Debug($"{controllerName.ToLower()}_{methodName.ToLower()}_notfound datafile - {errorMetadata}");
                return Content(System.Net.HttpStatusCode.NotFound, "DataFile not found");
            }
            catch (DatasetUnauthorizedAccessException)
            {
                Logger.Debug($"{controllerName.ToLower()}_{methodName.ToLower()}_unauthorizedaccess dataset - {errorMetadata}");
                return Content(System.Net.HttpStatusCode.Forbidden, "Unauthorized Access to Dataset");
            }
            catch (SchemaUnauthorizedAccessException)
            {
                Logger.Debug($"{controllerName.ToLower()}_{methodName.ToLower()}_unauthorizedexception schema - {errorMetadata}");
                return Content(System.Net.HttpStatusCode.Forbidden, "Unauthorized Access to Schema");
            }
            catch (SchemaConversionException ex)
            {
                Logger.Warn($"{controllerName.ToLower()}_{methodName.ToLower()}_schemaconversionexception - {errorMetadata}", ex);
                return Content(System.Net.HttpStatusCode.BadRequest, ex.Message);
            }
            catch (DataFileUnauthorizedException ex)
            {
                Logger.Warn($"{controllerName.ToLower()}_{methodName.ToLower()}_unauthorizedexception datasetfile - {errorMetadata}", ex);
                return Content(System.Net.HttpStatusCode.Forbidden, "Unauthorized Access to Data File");
            }
            catch (Exception ex)
            {
                Logger.Error($"{controllerName.ToLower()}_{methodName.ToLower()}_internalservererror - {errorMetadata}", ex);
                //Logger.Error($"metadataapi_getschemabydataset_internalservererror", ex);
                return InternalServerError(ex);
            }

        }

    }
}

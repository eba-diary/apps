using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Factories.Fields;
using Sentry.data.Web.Models.ApiModels.Dataset;
using Sentry.data.Web.Models.ApiModels.Schema;
using Sentry.WebAPI.Versioning;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Sentry.data.Web.WebApi.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_METADATA)]
    //Users need aleast UseApp permission to access any endpoint on this controller
    [WebApiAuthorizeUseApp]
    public class DataInventoryController : BaseWebApiController
    {

       
        [HttpGet]
        [ApiVersionBegin(Sentry.data.Web.WebAPI.Version.v2)]
        [Route("DoesItContainSensitive")]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, null, typeof(bool))]
        [SwaggerResponse(System.Net.HttpStatusCode.InternalServerError, null, null)]
        public async Task<IHttpActionResult> DoesItContainSensitive()
        {
            return Ok(true);
        }




    }
}

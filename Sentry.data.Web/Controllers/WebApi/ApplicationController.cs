﻿using System.Net;
using System.Web.Http;
using Sentry.data.Core;
using Swashbuckle.Swagger.Annotations;

namespace Sentry.data.Web.Controllers
{
    [RoutePrefix(WebConstants.Routes.VERSION_APPLICATIONS)]
    [AuthorizeByPermission(GlobalConstants.PermissionCodes.ADMIN_USER)]
    public class ApplicationController : BaseWebApiController
    {

        /// <summary>
        /// Gets all applicaitons
        /// </summary>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("")]
        public IHttpActionResult GetApplications()
        {
            return NoContent();
        }

        /// <summary>
        /// Gets specific application
        /// </summary>
        /// <param name="appId"></param>
        [HttpGet]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("{appId}")]
        public IHttpActionResult GetApplication(int appId)
        {
            return NoContent();
        }

        /// <summary>
        /// Create application
        /// </summary>
        /// <param name="appId"></param>
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("{appId}")]
        public IHttpActionResult CreateApplication(int appId)
        {
            return NoContent();
        }

        /// <summary>
        /// Update application
        /// </summary>
        /// <param name="appId"></param>
        [HttpPut]
        [SwaggerResponseRemoveDefaults]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof())]
        [Route("{appId}")]
        public IHttpActionResult UpdateApplication(int appId)
        {
            return NoContent();
        }


    }
}
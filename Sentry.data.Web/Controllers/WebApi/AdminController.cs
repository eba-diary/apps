using Sentry.data.Core;
using Sentry.data.Web.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Web.Models.ApiModels.Admin;
using Sentry.data.Core.Interfaces;
using System.Web.Http;
using Sentry.data.Core.DTO.Admin;
using Sentry.data.Web.Extensions;

namespace Sentry.data.Web.Controllers.WebApi
{
    public class AdminController : BaseWebApiController
    {
        private readonly ISupportLink _supportLinkService;

        public AdminController(ISupportLink supportLinkService)
        {
            _supportLinkService = supportLinkService;
        }

        private ISupportLink SupportLinkService
        {
            get { return _supportLinkService; }
        }

        public IHttpActionResult AddSupportLink(SupportLinkModel supportLinkModel) 
        {
            // the case when name or url is null
            if(supportLinkModel.Name == null || supportLinkModel.Url == null)
            {
                if(supportLinkModel.Name == null)
                {
                    return Content(System.Net.HttpStatusCode.BadRequest, $"Name was not submitted");
                }
                if(supportLinkModel.Url == null)
                {
                    return Content(System.Net.HttpStatusCode.BadRequest, "Url was not submitted");
                }
            }
            SupportLinkDto supportLinkDto = supportLinkModel.ToDto();
            try
            {
                SupportLinkService.AddSupportLink(supportLinkDto);
            } catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, "Error occured when adding support link to database");
            }
            
            return Ok();
        }

        public IHttpActionResult DeleteSupportLink(int id)
        {
            try
            {
                SupportLinkService.RemoveSupportLink(id);
            } catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, "Error occured when removing support link to database");
            }

            return Ok();
        }

       
    }
}
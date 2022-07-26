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
            if(supportLinkModel.Name == null || supportLinkModel.Url == null)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, "Invalid parameters passed");
            }
            SupportLinkDto supportLinkDto = new SupportLinkDto()
            {
                SupportLinkId = supportLinkModel.SupportLinkId,
                Name = supportLinkModel.Name,
                Description = supportLinkModel.Description,
                Url = supportLinkModel.Url,
            };
            SupportLinkService.AddSupportLink(supportLinkDto);
            // if true return ok otherwise return content has a bad request
        }

        public IHttpActionResult DeleteSupportLink(int id)
        {
            try
            {

            }
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
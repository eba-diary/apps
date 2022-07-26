using Sentry.data.Core.DTO.Admin;
using Sentry.data.Web.Models.ApiModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Extensions
{
    public static class AdminExtensions
    {
        public static SupportLinkDto ToDto(this SupportLinkModel model)
        {
            return new SupportLinkDto()
            {
                SupportLinkId = model.SupportLinkId,
                Name = model.Name,
                Description = model.Description,
                Url = model.Url,
            };
        }
    }
}
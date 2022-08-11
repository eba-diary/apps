using Sentry.data.Core;
using Sentry.data.Core.DTO.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Extensions
{
    public static class AdminExtensions
    {

        public static SupportLinkDto ToDto(this SupportLinkModel supportLinkModel)
        {
            SupportLinkDto dto = new SupportLinkDto()
            {
                Name = supportLinkModel.Name,
                Description = supportLinkModel.Description,
                Url = supportLinkModel.Url,
            };
            return dto;
        }

        public static SupportLinkModel ToModel(this SupportLinkDto supportLinkDto)
        {
            SupportLinkModel model = new SupportLinkModel()
            {
                Name = supportLinkDto.Name,
                Description = supportLinkDto.Description,
                Url = supportLinkDto.Url,
            };
            return model;
        }
    }


}
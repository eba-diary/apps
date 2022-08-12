using Sentry.data.Core.DTO.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Extensions
{
    public static class AdminExtensions
    {
        public static List<SupportLinkDto> ToDto(this List<SupportLink> supportLinkList)
        {
            List<SupportLinkDto> supportLinkDtos = new List<SupportLinkDto>();
            foreach (var supportLink in supportLinkList)
            {
                SupportLinkDto supportLinkDto = new SupportLinkDto()
                {
                    Name = supportLink.Name,
                    Description = supportLink.Description,
                    Url = supportLink.Url,
                };
                supportLinkDtos.Add(supportLinkDto);
            }
            return supportLinkDtos;
        }
    }
}

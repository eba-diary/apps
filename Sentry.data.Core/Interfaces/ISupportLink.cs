using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.DTO.Admin;

namespace Sentry.data.Core
{
    public interface ISupportLink
    {
        void AddSupportLink(SupportLinkDto supportLinkDto);
        void RemoveSupportLink(int id);
        List<SupportLinkDto> GetSupportLinks();
    }
}

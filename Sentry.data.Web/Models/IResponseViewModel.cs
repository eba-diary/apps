using System.Collections.Generic;

namespace Sentry.data.Web
{
    public interface IResponseViewModel
    {
        List<ResponseLinkViewModel> Links { get; set; }
        void SetLinks();
    }
}
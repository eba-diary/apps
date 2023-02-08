using System.Collections.Generic;

namespace Sentry.data.Web
{
    public abstract class SampleResponseViewModel : AddSampleViewModel, IResponseViewModel
    {
        public int Id { get; set; }
        public List<ResponseLinkViewModel> Links { get; set; }

        public abstract void SetLinks();
    }
}
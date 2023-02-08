using System.Collections.Generic;

namespace Sentry.data.Web
{
    public abstract class SampleViewModel : IRequestViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
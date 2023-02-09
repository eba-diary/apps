using System.Collections.Generic;

namespace Sentry.data.Web
{
    public abstract class SampleViewModel : IRequestModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
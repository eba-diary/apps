using System.Collections.Generic;

namespace Sentry.data.Web
{
    public abstract class SampleResponseViewModel : AddSampleViewModel, IResponseModel
    {
        public int Id { get; set; }
    }
}
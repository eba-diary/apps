using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class GetSampleResponseViewModel : SampleResponseViewModel
    {
        public override void SetLinks()
        {
            Links = new List<ResponseLinkViewModel>
            {
                new ResponseLinkViewModel
                {
                    Relationship = "Update",
                    Uri = $"api/v{WebAPI.Version.CURRENT}{WebConstants.Routes.SAMPLES}/{Id}",
                    HttpMethod = "PUT"
                }
            };
        }
    }
}
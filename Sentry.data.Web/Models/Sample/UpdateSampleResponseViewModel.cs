using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class UpdateSampleResponseViewModel : SampleResponseViewModel
    {
        public override void SetLinks()
        {
            Links = new List<ResponseLinkViewModel>
            {
                new ResponseLinkViewModel
                {
                    Relationship = "Get",
                    Uri = $"api/v{WebAPI.Version.CURRENT}{WebConstants.Routes.SAMPLES}/{Id}",
                    HttpMethod = "GET"
                }
            };
        }
    }
}
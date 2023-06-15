namespace Sentry.data.Web.API
{
    public class AddAssistanceResponseModel : IResponseModel
    {
        public string IssueKey { get; set; }
        public string IssueLink { get; set; }
    }
}
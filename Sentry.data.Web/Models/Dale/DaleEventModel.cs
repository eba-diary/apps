namespace Sentry.data.Web.Models.Dale
{
    public class DaleEventModel
    {
        public string Criteria { get; set; }
        public string Destiny { get; set; }
        public int QuerySeconds { get; set; }
        public int QueryRows { get; set; }
        public bool QuerySuccess { get; set; }
        public string QueryErrorMessage { get; set; }
        public string Sensitive { get; set; }
    }
}